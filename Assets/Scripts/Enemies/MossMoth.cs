using UnityEngine;

public class MossMoth : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float accelerationMax = 5f;
    [SerializeField] private float dampener = 0.9f;
    [SerializeField] private bool songMode = false;

    private float startX;
    private float startY;
    private bool flyingAway = false;
    private Vector2 currentVelocity;

    private void Awake()
    {
        startX = transform.position.x;
        startY = transform.position.y;
    }

    protected void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        if (!flyingAway && !songMode)
        {
            Buzz(deltaTime);
        }
        if (songMode)
        {
            // Vibrates in place when in song mode
            Vector3 vector = new Vector3(startX + Random.Range(-0.06f, 0.06f), startY + Random.Range(-0.06f, 0.06f), transform.position.z);
            transform.position = vector;
        }
    }

    private void Buzz(float deltaTime)
    {
        currentVelocity += new Vector2(Random.Range(-accelerationMax, accelerationMax), Random.Range(-accelerationMax, accelerationMax)) * deltaTime;
        currentVelocity *= dampener;

        transform.position += (Vector3)currentVelocity * deltaTime;

        // Gentle pull towards starting position so it doesn't drift away infinitely
        Vector2 pullBack = new Vector2(startX - transform.position.x, startY - transform.position.y) * deltaTime;
        currentVelocity += pullBack;
    }
}