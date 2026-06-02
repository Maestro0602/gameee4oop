using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D))]
public class ArcMeleeHitbox : MonoBehaviour
{
    [Header("Arc Angles (Degrees)")]
    [SerializeField] private float normalArc = 120f;
    [SerializeField] private float upwardArc = 90f;
    [SerializeField] private float downwardArc = 90f;

    [Header("Hitbox Properties")]
    [SerializeField] private int segments = 12;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float activeDuration = 0.15f;
    [SerializeField] private float baseDamage = 10f;

    [Header("Anchor")]
    [Tooltip("Assign the AttackOrigin child Transform from Hero. " +
             "The hitbox will snap to this world position on every attack, " +
             "which keeps it immune to parent scale flipping.")]
    [SerializeField] private Transform attackOrigin;

    private PolygonCollider2D col;
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();
    private Coroutine _activeRoutine;

    private void Awake()
    {
        col = GetComponent<PolygonCollider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    public void PerformAttack(HeroController.AttackDirection dir, bool facingRight)
    {
        alreadyHit.Clear();

        // Snap to attackOrigin in world space so parent scale-flip never
        // shifts the origin sideways.
        if (attackOrigin != null)
            transform.position = attackOrigin.position;

        GenerateArc(dir, facingRight);

        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(ActiveRoutine());
    }

    private void GenerateArc(HeroController.AttackDirection dir, bool facingRight)
    {
        float arc = GetArcAngle(dir);
        float centerAngle = GetCenterAngle(dir, facingRight);

        float startAngle = centerAngle - (arc / 2f);
        float step = arc / segments;

        Vector2[] points = new Vector2[segments + 2];
        points[0] = Vector2.zero;

        // Preserve sign of lossyScale so a negative parent scale (used for
        // sprite flipping) is respected rather than cancelled by Mathf.Abs.
        float scaleX = transform.lossyScale.x;
        float scaleY = transform.lossyScale.y;

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + step * i;
            float rad = angle * Mathf.Deg2Rad;

            float px = scaleX != 0f ? (Mathf.Cos(rad) * radius) / scaleX : 0f;
            float py = scaleY != 0f ? (Mathf.Sin(rad) * radius) / scaleY : 0f;

            points[i + 1] = new Vector2(px, py);
        }

        col.SetPath(0, points);
    }

    private float GetArcAngle(HeroController.AttackDirection dir)
    {
        return dir switch
        {
            HeroController.AttackDirection.upward => upwardArc,
            HeroController.AttackDirection.downward => downwardArc,
            _ => normalArc
        };
    }

    private float GetCenterAngle(HeroController.AttackDirection dir, bool facingRight)
    {
        return dir switch
        {
            HeroController.AttackDirection.upward => 90f,
            HeroController.AttackDirection.downward => -90f,
            // Flip center angle based on facing so the arc always
            // swings in the direction the character is looking.
            _ => facingRight ? 0f : 180f
        };
    }

    private IEnumerator ActiveRoutine()
    {
        col.enabled = true;
        yield return new WaitForSeconds(activeDuration);
        col.enabled = false;
        _activeRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyHit.Contains(other)) return;

        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            Vector2 hitDir = (other.transform.position - transform.position).normalized;
            dmg.TakeDamage(baseDamage, hitDir);
            alreadyHit.Add(other);
            return;
        }

        // Uncomment once the "Enemy" tag exists in
        // Project Settings -> Tags and Layers.
        // if (other.CompareTag("Enemy"))
        // {
        //     Vector2 hitDir = (other.transform.position - transform.position).normalized;
        //     other.SendMessage("TakeDamage", new object[] { baseDamage, hitDir },
        //         SendMessageOptions.DontRequireReceiver);
        //     alreadyHit.Add(other);
        // }
    }
}