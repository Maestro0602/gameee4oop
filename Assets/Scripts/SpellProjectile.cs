using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SpellProjectile : MonoBehaviour
{
    [Header("Spell Settings")]
    public float speed = 15f;
    public int damage = 1;
    public float lifetime = 2f;
    
    [Tooltip("If true, the spell goes through multiple enemies until it hits a wall.")]
    public bool pierceEnemies = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        
        // Destroy automatically after 'lifetime' seconds if it doesn't hit anything
        Destroy(gameObject, lifetime);
    }

    // Called by HeroController when spawning the spell to set its direction
    public void Fire(int facingDirection)
    {
        // Flip the sprite if shooting left
        if (facingDirection < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Apply velocity
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(speed * facingDirection, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Don't hit the player
        if (collision.CompareTag("Player") || collision.gameObject.GetComponent<HeroController>() != null) return;
        
        // Don't hit triggers (like shop items or currency)
        if (collision.isTrigger && !collision.CompareTag("Enemy")) return;

        bool hitEnemy = collision.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy");

        if (hitEnemy)
        {
            HealthManager hp = collision.GetComponent<HealthManager>();
            if (hp == null) hp = collision.GetComponentInParent<HealthManager>();
            
            if (hp != null)
            {
                // Deal damage (hit angle doesn't matter much for spells usually, so we pass 0)
                hp.TakeHit(new HitInstance(this.gameObject, damage, 0f, 0f));
                
                // If it doesn't pierce, destroy it on the first enemy hit
                if (!pierceEnemies)
                {
                    Explode();
                }
            }
        }
        else
        {
            // Hit a wall or floor (assuming it's not a trigger and not an enemy)
            Explode();
        }
    }

    private void Explode()
    {
        // Optional: Instantiate a particle explosion effect here
        // Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}
