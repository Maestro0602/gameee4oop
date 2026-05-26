using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Centipede : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayDistance = 0.5f;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Coroutine crawlRoutine;
    private int facingDirection = 1; // 1 for right, -1 for left

    public bool IsTurning { get; private set; }
    public bool IsCrawling => crawlRoutine != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        // GLOBAL FIX: This stops raycasts from detecting the collider they originate inside of.
        // This is crucial if everything (including this enemy) is on the "Default" layer.
        Physics2D.queriesStartInColliders = false;
    }

    private void Start()
    {
        StartCrawling();
    }

    public void StartCrawling()
    {
        if (crawlRoutine == null)
        {
            crawlRoutine = StartCoroutine(CrawlRoutine());
        }
    }

    public void StopCrawling()
    {
        if (crawlRoutine != null)
        {
            StopCoroutine(crawlRoutine);
            crawlRoutine = null;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private IEnumerator CrawlRoutine()
    {
        while (true)
        {
            // Wait for the physics frame first so we don't evaluate positions prematurely
            yield return new WaitForFixedUpdate();

            if (IsTurning) continue;

            // Move forward
            rb.linearVelocity = new Vector2(speed * facingDirection, rb.linearVelocity.y);

            // Check if we need to turn
            if (CheckWall() || !CheckGround())
            {
                yield return StartCoroutine(TurnRoutine());
            }
        }
    }

    private bool CheckGround()
    {
        if (coll == null) return true;

        float inset = 0.1f;
        float xPos = facingDirection == 1 ? coll.bounds.max.x - inset : coll.bounds.min.x + inset;

        Vector2 origin = new Vector2(xPos, coll.bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundLayer);

        // Explicitly ignore our own collider just in case
        return hit.collider != null && hit.collider != coll;
    }

    private bool CheckWall()
    {
        if (coll == null) return false;

        // Push the origin slightly outside the collider bounds depending on direction
        float xPos = facingDirection == 1 ? coll.bounds.max.x + 0.05f : coll.bounds.min.x - 0.05f;
        Vector2 origin = new Vector2(xPos, coll.bounds.center.y);
        Vector2 direction = facingDirection == 1 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, groundLayer);

        // Explicitly ignore our own collider just in case
        return hit.collider != null && hit.collider != coll;
    }

    private IEnumerator TurnRoutine()
    {
        IsTurning = true;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        facingDirection *= -1;

        // Clean scale flipping logic
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * facingDirection;
        transform.localScale = localScale;

        // Cooldown gives the centipede time to step away from the wall
        yield return new WaitForSeconds(0.2f);

        IsTurning = false;
    }

    private void OnDrawGizmos()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c == null) return;

        Gizmos.color = Color.red;
        float xPos = facingDirection == 1 ? c.bounds.max.x : c.bounds.min.x;

        Vector2 groundOrigin = new Vector2(facingDirection == 1 ? c.bounds.max.x - 0.1f : c.bounds.min.x + 0.1f, c.bounds.min.y);
        Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * rayDistance);

        Vector2 wallOrigin = new Vector2(facingDirection == 1 ? c.bounds.max.x + 0.05f : c.bounds.min.x - 0.05f, c.bounds.center.y);
        Vector2 wallDir = facingDirection == 1 ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(wallOrigin, wallOrigin + wallDir * rayDistance);
    }
}