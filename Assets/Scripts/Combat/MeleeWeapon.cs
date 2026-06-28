using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The physical sword hitbox. Attach to a child of Hero with a trigger Collider2D.
/// When enabled, it detects enemies and applies damage + recoil/pogo.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MeleeWeapon : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackForce = 15f;

    private Collider2D weaponCollider;
    private List<Collider2D> alreadyHit = new List<Collider2D>();

    public bool IsActive => weaponCollider != null && weaponCollider.enabled;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider2D>();
        weaponCollider.enabled = false;
        weaponCollider.isTrigger = true;
    }

    public void EnableWeapon()
    {
        weaponCollider.enabled = true;
        alreadyHit.Clear();
    }

    public void DisableWeapon()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyHit.Contains(other)) return;

        // Find HealthManager on the object or its parent
        HealthManager hp = other.GetComponent<HealthManager>();
        if (hp == null) hp = other.GetComponentInParent<HealthManager>();
        if (hp == null) return;

        alreadyHit.Add(other);

        // Hit direction based on player facing / attack direction
        float hitAngle = HeroController.instance.FacingDirection == 1 ? 0f : 180f;
        if (HeroController.instance.cState.upAttacking) hitAngle = 90f;
        else if (HeroController.instance.cState.downAttacking) hitAngle = 270f;

        hp.TakeHit(new HitInstance(
            HeroController.instance.gameObject,
            damageAmount,
            knockbackForce,
            hitAngle
        ));

        // --- Hollow Knight Recoil ---
        // Pogo: down-attacking in the air bounces you up
        if (HeroController.instance.cState.downAttacking && !HeroController.instance.cState.onGround)
        {
            HeroController.instance.Recoil(Vector2.up);
        }
        // Horizontal recoil: normal slash pushes you back
        else if (!HeroController.instance.cState.upAttacking)
        {
            HeroController.instance.Recoil(new Vector2(-HeroController.instance.FacingDirection, 0));
        }
    }

    // -------------------------------------------------------
    // Debug Visualizer
    // -------------------------------------------------------
    private void OnDrawGizmos()
    {
        // If we haven't assigned it in Awake yet, try to get it
        Collider2D col = weaponCollider != null ? weaponCollider : GetComponent<Collider2D>();
        if (col == null) return;

        // Draw solid red when attacking, faint wireframe when idle
        if (Application.isPlaying && col.enabled)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent Red
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.1f); // Very faint Red
        }

        Vector3 pos = col.transform.position;
        
        if (col is BoxCollider2D box)
        {
            // Calculate real center considering offset and scale
            Vector3 center = pos + (Vector3)(box.offset * col.transform.lossyScale);
            Vector3 size = new Vector3(box.size.x * col.transform.lossyScale.x, box.size.y * col.transform.lossyScale.y, 0);
            
            if (Application.isPlaying && col.enabled) Gizmos.DrawCube(center, size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
        else if (col is PolygonCollider2D poly)
        {
            // Draw polygon paths
            Vector3 scale = col.transform.lossyScale;
            Vector2 offset = poly.offset;
            
            for (int i = 0; i < poly.pathCount; i++)
            {
                Vector2[] path = poly.GetPath(i);
                for (int j = 0; j < path.Length; j++)
                {
                    Vector3 p1 = pos + new Vector3((path[j].x + offset.x) * scale.x, (path[j].y + offset.y) * scale.y, 0);
                    Vector3 p2 = pos + new Vector3((path[(j + 1) % path.Length].x + offset.x) * scale.x, (path[(j + 1) % path.Length].y + offset.y) * scale.y, 0);
                    
                    Gizmos.DrawLine(p1, p2);
                }
            }
            
            // To simulate filled polygon, we draw lines from center to edges if active
            if (Application.isPlaying && col.enabled)
            {
                Vector2[] path = poly.GetPath(0);
                Vector3 center = pos + (Vector3)(offset * scale);
                for (int j = 0; j < path.Length; j++)
                {
                    Vector3 p = pos + new Vector3((path[j].x + offset.x) * scale.x, (path[j].y + offset.y) * scale.y, 0);
                    Gizmos.DrawLine(center, p);
                }
            }
        }
        else if (col is CircleCollider2D circle)
        {
            Vector3 center = pos + (Vector3)(circle.offset * col.transform.lossyScale);
            float radius = circle.radius * Mathf.Max(Mathf.Abs(col.transform.lossyScale.x), Mathf.Abs(col.transform.lossyScale.y));
            
            if (Application.isPlaying && col.enabled) Gizmos.DrawSphere(center, radius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
