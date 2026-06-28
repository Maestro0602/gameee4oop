using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BombardierBeetle : MonoBehaviour
{
    [Header("Charge Attack")]
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float aggroRange = 10f;
    [SerializeField] private Transform playerTransform; // Assign in inspector or gets by tag

    private Rigidbody2D rb;
    private HealthManager healthManager;
    private bool isCharging;
    private int facingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthManager = GetComponent<HealthManager>();
    }

    private void Start()
    {
        if (playerTransform == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance < aggroRange && !isCharging)
        {
            StartCharge();
        }
    }

    private void FixedUpdate()
    {
        // Skip movement while being knocked back — let the knockback velocity play out
        if (healthManager != null && healthManager.IsKnockedBack)
        {
            isCharging = false; // Cancel charge on hit
            return;
        }

        if (isCharging)
        {
            rb.linearVelocity = new Vector2(chargeSpeed * facingDirection, rb.linearVelocity.y);
        }
    }

    private void StartCharge()
    {
        isCharging = true;
        // Determines direction based on player X position vs beetle X position
        facingDirection = playerTransform.position.x > transform.position.x ? 1 : -1;

        // Face the correct direction visually
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (facingDirection == 1 ? -1 : 1);  // Flips based on direction
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Stop charging if it hits a wall
        if (isCharging)
        {
            isCharging = false;
        }
    }
}