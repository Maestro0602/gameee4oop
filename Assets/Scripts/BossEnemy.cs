using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Attack Settings")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float rangeAttackRange = 8f;
    [SerializeField] private float attackCooldown = 2f;

    private float currentHealth;
    private float attackTimer;
    private bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        attackTimer -= Time.deltaTime;

        if (distanceToPlayer <= meleeRange)
        {
            MeleeAttack();
        }
        else if (distanceToPlayer <= rangeAttackRange)
        {
            RangeAttack();
        }
        else
        {
            MoveTowardsPlayer();
        }

        FlipTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    private void MeleeAttack()
    {
        animator.SetBool("isWalking", false);
        if (attackTimer <= 0)
        {
            Debug.Log("Melee Attack!");
            animator.SetTrigger("attack");
            attackTimer = attackCooldown;
        }
    }

    private void RangeAttack()
    {
        animator.SetBool("isWalking", false);
        if (attackTimer <= 0)
        {
            Debug.Log("Range Attack!");
            animator.SetTrigger("rangeAttack");
            attackTimer = attackCooldown;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("death");
        Destroy(gameObject, 2f);
    }

    private void FlipTowardsPlayer()
    {
        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}

