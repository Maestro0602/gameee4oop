using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class HealthManager : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Physics")]
    [Tooltip("If true, the enemy's mass is set very high so the player cannot push it around.")]
    [SerializeField] private bool preventPhysicsPush = true;
    [Tooltip("Mass applied to resist player pushes. Higher = harder to push.")]
    [SerializeField] private float heavyMass = 10000f;

    private Rigidbody2D rb;
    private bool isKnockedBack;
    private float originalMass;

    // Cached MonoBehaviours that drive AI movement (PlayMaker FSMs, custom AI scripts, etc.)
    // These get paused during knockback so they don't fight the knockback velocity.
    private MonoBehaviour[] cachedAIComponents;

    /// <summary>True while the enemy is being knocked back. Enemy AI should skip movement when this is true.</summary>
    public bool IsKnockedBack => isKnockedBack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        originalMass = rb.mass;
    }

    private void Start()
    {
        // Fix for prefabs that saved a 0 knockback duration before the script was updated
        if (knockbackDuration <= 0f) knockbackDuration = 0.2f;

        // Set very high mass so the player cannot push this enemy like a trolley.
        // High mass does NOT affect gravity (acceleration = g regardless of mass in Unity).
        // High mass does NOT affect script-set velocity (rb.linearVelocity = X works the same).
        // High mass ONLY reduces collision response (player bounces off instead of pushing).
        if (preventPhysicsPush && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.mass = heavyMass;
        }

        // Cache any PlayMakerFSM components on this enemy for pausing during knockback.
        // PlayMaker's MoveTowards action sets transform.position directly, which fights
        // with velocity-based knockback. We disable them briefly during knockback.
        CacheAIComponents();
    }

    private void CacheAIComponents()
    {
        // Find all MonoBehaviours whose type name contains "PlayMakerFSM"
        // This avoids a hard compile-time dependency on the PlayMaker assembly.
        var allBehaviours = GetComponents<MonoBehaviour>();
        var list = new System.Collections.Generic.List<MonoBehaviour>();
        foreach (var mb in allBehaviours)
        {
            if (mb == null || mb == this) continue;
            string typeName = mb.GetType().Name;
            if (typeName == "PlayMakerFSM")
            {
                list.Add(mb);
            }
        }
        cachedAIComponents = list.Count > 0 ? list.ToArray() : null;
    }

    // Required by IDamageable interface if used elsewhere
    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        HitInstance hit = new HitInstance(gameObject, (int)amount, 10f, Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg);
        TakeHit(hit);
    }

    // Main hit function called by HeroController
    public void TakeHit(HitInstance hit)
    {
        currentHealth -= hit.DamageAmount;
        Debug.Log($"[HealthManager] {gameObject.name} took {hit.DamageAmount} damage! HP: {currentHealth}/{maxHealth}.");

        if (currentHealth <= 0)
        {
            Debug.Log($"[HealthManager] {gameObject.name} has been defeated!");
            Destroy(gameObject);
            return;
        }

        Vector2 hitDirection = new Vector2(
            Mathf.Cos(hit.DirectionAngle * Mathf.Deg2Rad), 
            Mathf.Sin(hit.DirectionAngle * Mathf.Deg2Rad)
        );

        ApplyKnockback(hitDirection, hit.KnockbackForce);
    }

    private void ApplyKnockback(Vector2 direction, float forceMagnitude)
    {
        if (rb != null && gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(KnockbackRoutine(direction, forceMagnitude));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float forceMagnitude)
    {
        isKnockedBack = true;

        // Pause AI components (PlayMaker FSMs etc.) so they don't override knockback
        SetAIComponentsEnabled(false);

        // Calculate knockback velocity
        float dirSign = Mathf.Abs(direction.x) > 0.01f ? Mathf.Sign(direction.x) : 1f;
        Vector2 force = new Vector2(dirSign * forceMagnitude, 4f);
        
        Debug.Log($"[HealthManager] {gameObject.name} knockback START. Force: {force}, Duration: {knockbackDuration}s");
        
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = force;
        }

        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Resume AI components
        SetAIComponentsEnabled(true);

        isKnockedBack = false;
        Debug.Log($"[HealthManager] {gameObject.name} knockback END.");
    }

    private void SetAIComponentsEnabled(bool enabled)
    {
        if (cachedAIComponents == null) return;
        foreach (var comp in cachedAIComponents)
        {
            if (comp != null)
                comp.enabled = enabled;
        }
    }
}
