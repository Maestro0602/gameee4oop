using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FoxballControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float force = 15f;
    public float maxLifeTime = 5f;
    public float tweenY = 1.5f;

    private Rigidbody2D body;
    private float startY;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
    }

    private void OnEnable()
    {
        StartCoroutine(DoFire());
    }

    private IEnumerator DoFire()
    {
        // Simple Sine wave implementation (replacing iTween for safety if not installed)
        float elapsedTime = 0f;
        float randomOffset = Random.Range(-0.2f, 0.2f);
        float targetY = tweenY + randomOffset;
        
        Vector2 startPos = transform.position;

        while (elapsedTime < maxLifeTime)
        {
            elapsedTime += Time.fixedDeltaTime;

            // Apply horizontal force via Rigidbody2D
            body.AddForce(new Vector2(force, 0f), ForceMode2D.Force);

            // Vertical sway
            float sway = Mathf.Sin((elapsedTime / 0.7f) * Mathf.PI * 2f) * targetY;
            Vector3 pos = transform.position;
            pos.y = startY + sway;
            transform.position = pos;

            yield return new WaitForFixedUpdate();
        }

        DoHit();
    }

    private void DoHit()
    {
        // Destroy or pool fireball
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<HeroController>() != null)
        {
            // Damage player logic here
            Debug.Log("Foxball hit the player!");
            DoHit();
        }
        else if (collision.CompareTag("Ground"))
        {
            DoHit();
        }
    }
}
