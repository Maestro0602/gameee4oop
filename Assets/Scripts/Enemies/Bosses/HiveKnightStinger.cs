using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HiveKnightStinger : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public float speed = 20f;
    public float direction = 0f; // in degrees
    public float timer = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Calculate trajectory X and Y velocities using sine and cosine functions
        float rad = this.direction * Mathf.Deg2Rad; // 0.017453292f
        float num = this.speed * Mathf.Cos(rad);
        float num2 = this.speed * Mathf.Sin(rad);
        
        Vector2 vector = new Vector2(num, num2);
        this.rb.linearVelocity = vector; // Apply velocity to physics body

        // Rotate sprite to face direction
        transform.rotation = Quaternion.Euler(0, 0, direction);

        // Deactivate projectile after timer expires
        if (this.timer > 0f)
        {
            this.timer -= Time.deltaTime;
            return;
        }
        
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<HeroController>() != null)
        {
            // Damage player logic here
            Debug.Log("HiveKnightStinger hit the player!");
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
