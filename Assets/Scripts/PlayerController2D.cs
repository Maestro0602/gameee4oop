using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airControlMultiplier = 0.6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float moveInput;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 3.5f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveInput = ReadHorizontalInput();

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (ReadJumpPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float speedDifference = targetSpeed - rb.linearVelocity.x;

        float controlMultiplier = isGrounded ? 1f : airControlMultiplier;
        float currentAccelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        float movement = speedDifference * currentAccelRate * controlMultiplier * Time.fixedDeltaTime;
        float newXVelocity = Mathf.Clamp(rb.linearVelocity.x + movement, -moveSpeed, moveSpeed);

        rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            if (moveInput > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    private float ReadHorizontalInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float horizontal = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            return horizontal;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxisRaw("Horizontal");
#else
        return 0f;
#endif
    }

    private bool ReadJumpPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump");
#else
        return false;
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
