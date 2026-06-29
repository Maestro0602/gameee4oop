using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float lifetime = 3f;

    [Header("Effects")]
    public GameObject hitEffectPrefab;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-destroy after a few seconds so it doesn't fly forever
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Move the projectile forward based on its local right vector
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore other enemies or boss attacks
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") || collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            return;
        }

        // Hit the Player
        if (collision.CompareTag("Player") && HeroController.instance != null)
        {
            HeroController.instance.TakeDamageAndKnockback(transform.position);
        }

        // Spawn explosion/hit effect if we have one
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy the projectile when it hits something solid or the player
        Destroy(gameObject);
    }
}
