using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TinyMossFly : MonoBehaviour
{
    [Header("Roaming Settings")]
    public float waitMin = 0.5f;
    public float waitMax = 1.5f;
    public float roamingRange = 5f;
    public float accelerationMax = 10f;
    public float speedMax = 3f;
    public float dampener = 1.1f;

    private Rigidbody2D body;
    private HealthManager healthManager;
    private Vector2 start2D;
    private float waitTimer;
    private Vector2 acceleration2D;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        healthManager = GetComponent<HealthManager>();
        start2D = body.position;
        waitTimer = Random.Range(waitMin, waitMax);
    }

    private void FixedUpdate()
    {
        // Skip movement while being knocked back — let the knockback velocity play out
        if (healthManager != null && healthManager.IsKnockedBack) return;

        Buzz(Time.fixedDeltaTime);
    }

    private void Buzz(float deltaTime)
    {
        Vector2 position = this.body.position;
        Vector2 linearVelocity = this.body.linearVelocity;
        bool flag;

        if (this.waitTimer <= 0f)
        {
            flag = true;
            this.waitTimer = Random.Range(this.waitMin, this.waitMax);
        }
        else
        {
            this.waitTimer -= deltaTime;
            flag = false;
        }

        for (int i = 0; i < 2; i++)
        {
            float num = linearVelocity[i];
            float num2 = this.start2D[i];
            float num3 = position[i] - num2;
            float num4 = this.acceleration2D[i];

            if (flag)
            {
                // Pivot acceleration direction if outside allowed roaming range
                if (Mathf.Abs(num3) > this.roamingRange)
                {
                    num4 = -Mathf.Sign(num3) * this.accelerationMax;
                }
                else
                {
                    num4 = Random.Range(-this.accelerationMax, this.accelerationMax);
                }
                num4 /= 2000f; // Scale down constant
            }
            else if (Mathf.Abs(num3) > this.roamingRange && (num3 > 0f) == (num > 0f))
            {
                // Apply dampening to smoothly decelerate near edges
                num4 = this.accelerationMax * -Mathf.Sign(num3) / 2000f;
                num /= this.dampener;
                this.waitTimer = Random.Range(this.waitMin, this.waitMax);
            }

            num += num4;
            num = Mathf.Clamp(num, -this.speedMax, this.speedMax);
            linearVelocity[i] = num;
            this.acceleration2D[i] = num4;
        }

        // Apply visual flip if moving left or right
        if (Mathf.Abs(linearVelocity.x) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = linearVelocity.x > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        this.body.linearVelocity = linearVelocity;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<HeroController>() != null)
        {
            // Do damage
            Debug.Log("TinyMossFly hit the player!");
        }
    }
}
