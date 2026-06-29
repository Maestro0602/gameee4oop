using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Princess : MonoBehaviour
{
    [Header("Boss Core Settings")]
    public float timeBetweenAttacks = 1.5f;
    public bool isFlipped = false; // Set to true if your sprite faces Left by default

    [Header("Movement & Dash Settings")]
    public float walkSpeed = 3f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;

    [Header("Teleport Settings")]
    public float teleportMinDistance = 3f;
    public float teleportMaxDistance = 8f;
    public LayerMask wallLayer;

    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    [Header("Ball Form Settings")]
    public Transform centerArenaPoint;
    public int ballFormWaves = 8;

    private Rigidbody2D rb;
    private Animator anim;
    private HealthManager health;
    private Transform player;

    private bool isDead = false;
    private bool isAttacking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<HealthManager>();
        
        // Find the player automatically
        if (HeroController.instance != null)
        {
            player = HeroController.instance.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Start the Boss brain!
        StartCoroutine(BossRoutine());
    }

    private void Update()
    {
        if (health != null && health.IsKnockedBack) return; 
    }

    private IEnumerator BossRoutine()
    {
        // Give player a second before boss starts attacking
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            if (player == null) yield break;

            // Wait before next attack
            yield return new WaitForSeconds(timeBetweenAttacks);

            if (health != null && health.IsKnockedBack)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            FacePlayer();
            isAttacking = true;

            // Pick a random attack! (0 to 5)
            int attackChoice = Random.Range(0, 6);

            switch (attackChoice)
            {
                case 0:
                    yield return StartCoroutine(FireballAttack());
                    break;
                case 1:
                    yield return StartCoroutine(MultiFireballAttack());
                    break;
                case 2:
                    yield return StartCoroutine(TeleportAttack());
                    break;
                case 3:
                    yield return StartCoroutine(DashAttack());
                    break;
                case 4:
                    yield return StartCoroutine(MeleeAttack());
                    break;
                case 5:
                    yield return StartCoroutine(BallFormAttack());
                    break;
            }

            isAttacking = false;
        }
    }

    private IEnumerator FireballAttack()
    {
        anim.SetTrigger("Fireball");
        yield return new WaitForSeconds(0.3f); 
        ShootFireball();
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator MultiFireballAttack()
    {
        anim.SetTrigger("MultiFireball");
        
        // Shoot 3 fireballs like Grimm!
        for(int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.3f);
            
            // Slightly random heights for the bats/fireballs
            Vector3 spawnOffset = new Vector3(0, Random.Range(-0.5f, 1f), 0);
            ShootFireball(spawnOffset);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void ShootFireball(Vector3 offset = default)
    {
        if (fireballPrefab != null && fireballSpawnPoint != null)
        {
            Quaternion rotation = transform.localScale.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180);
            Instantiate(fireballPrefab, fireballSpawnPoint.position + offset, rotation);
        }
    }

    private void ShootFireballDirection(Vector2 direction, Vector3 offset = default)
    {
        if (fireballPrefab != null && fireballSpawnPoint != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Instantiate(fireballPrefab, fireballSpawnPoint.position + offset, Quaternion.Euler(0, 0, angle));
        }
    }

    private IEnumerator BallFormAttack()
    {
        anim.SetTrigger("BallForm");
        
        // Wait for boss to transform
        yield return new WaitForSeconds(0.5f);

        // Teleport to center!
        if (centerArenaPoint != null)
        {
            transform.position = centerArenaPoint.position;
        }
        else
        {
            transform.position = new Vector3(0, transform.position.y + 3f, 0); // fallback if no point is set
        }

        // Shoot fireballs in alternating waves (Pufferfish attack)
        for(int i = 0; i < ballFormWaves; i++)
        {
            yield return new WaitForSeconds(0.4f);
            
            // Randomly pick if this wave is low or high
            float heightLeft = Random.value > 0.5f ? -0.5f : 1f;
            float heightRight = Random.value > 0.5f ? -0.5f : 1f;

            ShootFireballDirection(Vector2.left, new Vector3(0, heightLeft, 0));
            ShootFireballDirection(Vector2.right, new Vector3(0, heightRight, 0));
        }

        anim.SetTrigger("EndBallForm");
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator TeleportAttack()
    {
        anim.SetTrigger("TeleportOut");
        
        // Wait for boss to disappear
        yield return new WaitForSeconds(0.4f);

        // Pick a random side (left or right of player)
        float side = Random.value > 0.5f ? 1f : -1f;
        float randomDist = Random.Range(teleportMinDistance, teleportMaxDistance);
        
        Vector3 newPos = player.position + new Vector3(side * randomDist, 0, 0);
        newPos.y = transform.position.y; // Keep the boss on the same Y level
        
        // Raycast to prevent teleporting into walls
        Vector2 castStart = new Vector2(player.position.x, transform.position.y);
        RaycastHit2D hit = Physics2D.Linecast(castStart, newPos, wallLayer);
        
        if (hit.collider != null)
        {
            // If we hit a wall, place the boss slightly in front of it
            newPos.x = hit.point.x - (side * 1.5f); 
        }
        
        transform.position = newPos;
        FacePlayer();

        anim.SetTrigger("TeleportIn");
        
        // Wait for boss to reappear
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator MeleeAttack()
    {
        anim.SetTrigger("Melee");
        
        float direction = transform.position.x < player.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator DashAttack()
    {
        anim.SetTrigger("Dash");

        float direction = transform.position.x < player.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * dashSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        float absX = Mathf.Abs(scale.x);

        if (player.position.x > transform.position.x)
        {
            scale.x = isFlipped ? -absX : absX;
        }
        else if (player.position.x < transform.position.x)
        {
            scale.x = isFlipped ? absX : -absX;
        }

        transform.localScale = scale;
    }

    // --- CONTACT DAMAGE (Boss deals damage if you touch it, especially during dashes) ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        CheckDamagePlayer(collider.gameObject);
    }

    private void CheckDamagePlayer(GameObject obj)
    {
        if (obj.CompareTag("Player") && HeroController.instance != null)
        {
            // The HeroController has a built in invincibility timer so it won't hit every frame
            HeroController.instance.TakeDamageAndKnockback(transform.position);
        }
    }
}
