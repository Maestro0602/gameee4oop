using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GoldenWaspBossStinger : MonoBehaviour
{
    [Header("Projectile Properties")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float direction; // Setup direction in degrees
    [SerializeField] private float timer = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 0.017453292f is Mathf.Deg2Rad
        float num = speed * Mathf.Cos(direction * Mathf.Deg2Rad); // Calculate trajectory X 
        float num2 = speed * Mathf.Sin(direction * Mathf.Deg2Rad); // Calculate trajectory Y

        Vector2 vector = new Vector2(num, num2);
        rb.linearVelocity = vector; // Apply trajectory

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false); // Disappears after timer expires
        }
    }

    public void Init(float fireDirection, float initialSpeed)
    {
        direction = fireDirection;
        speed = initialSpeed;
        timer = 2f;
    }
}