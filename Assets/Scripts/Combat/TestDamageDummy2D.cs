using UnityEngine;

using UnityEngine;

[DisallowMultipleComponent]
public class TestDamageDummy2D : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private bool destroyOnDeath = true;

    [Header("Feedback")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.08f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor = Color.white;
    private float hitFlashTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (spriteRenderer == null || hitFlashTimer <= 0f)
        {
            return;
        }

        hitFlashTimer -= Time.deltaTime;
        if (hitFlashTimer <= 0f)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (amount <= 0f || currentHealth <= 0f)
        {
            return;
        }

        currentHealth -= amount;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            hitFlashTimer = hitFlashDuration;
        }

        if (rb != null)
        {
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        Debug.Log($"{name} took {amount} damage. HP: {Mathf.Max(currentHealth, 0f):0.##}/{maxHealth:0.##}");

        if (currentHealth <= 0f)
        {
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
