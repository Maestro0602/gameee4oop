using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class HeroController : MonoBehaviour
{
    public static HeroController instance;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("Movement - Run & Walk")]
    [SerializeField] private float RUN_SPEED = 8.3f;
    [SerializeField] private float WALK_SPEED = 6f;

    [Header("Movement - Jump")]
    [SerializeField] private float JUMP_SPEED = 18.5f;

    [Header("Movement - Double Jump")]
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private float doubleJumpSpeed = 20f;

    [Header("Movement - Dash")]
    [SerializeField] private float DASH_SPEED = 20f;
    [SerializeField] private float DASH_DISTANCE = 6f;
    [SerializeField] private float DASH_COOLDOWN = 0.6f;

    [Header("Movement - Wall")]
    [SerializeField] private float WALLJUMP_RAY_LENGTH = 1.2f;
    [SerializeField] private float WJ_KICKOFF_SPEED = 10f;
    [SerializeField] private float WALLSLIDE_ACCEL = 2.5f;
    [SerializeField] private float WALLSLIDE_STICK_TIME = 0.2f;
    [SerializeField] private float WALLCLING_DECEL = 0.5f;
    [SerializeField] private float WALLCLING_COOLDOWN = 0.5f;
    [Tooltip("Max fall speed while wall-sliding.")]
    [SerializeField] private float WALLSLIDE_SPEED = 3f;
    [Tooltip("Vertical speed applied during wall-jump.")]
    [SerializeField] private float WJ_JUMP_SPEED = 17f;
    [Tooltip("Brief input lockout after wall-jump (seconds).")]
    [SerializeField] private float WJ_INPUT_LOCKOUT = 0.15f;

    [Header("Movement - Smoothing")]
    [SerializeField] private float GROUND_ACCEL = 60f;
    [SerializeField] private float GROUND_DECEL = 45f;
    [SerializeField] private float AIR_ACCEL = 30f;
    [SerializeField] private float AIR_DECEL = 18f;

    [Header("Movement - Assist")]
    [Tooltip("Grace period after leaving ground where jump still works.")]
    [SerializeField] private float COYOTE_TIME = 0.1f;
    [Tooltip("How long a jump press is remembered before landing.")]
    [SerializeField] private float JUMP_BUFFER_TIME = 0.12f;

    [Header("Movement - Gravity")]
    [Tooltip("Gravity applied while falling or at apex with no input.")]
    [SerializeField] private float DEFAULT_GRAVITY = 3.2f;
    [Tooltip("Reduced gravity while ascending with jump held.")]
    [SerializeField] private float AIR_HANG_GRAVITY = 2.6f;
    [Tooltip("How much to multiply gravity when falling (snappier landing).")]
    [SerializeField] private float FALL_GRAVITY_MULTIPLIER = 1.2f;
    [SerializeField] private float MAX_FALL_VELOCITY = 20f;
    [SerializeField] private float MAX_FALL_VELOCITY_DJUMP = 15f;
    [Tooltip("How much to cut vertical speed when jump button is released early.")]
    [SerializeField] private float JUMP_CUT_MULTIPLIER = 0.6f;

    // Internal State
    private bool doubleJumped;
    private bool airDashed;
    private bool wasOnGround;
    private bool isDoubleJumping;     // track if last jump was a double jump (affects fall cap)

    // Wall & assist state
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float wallJumpLockoutTimer;
    private int wallDirection;           // -1 = wall on left, 1 = wall on right, 0 = no wall
    private bool touchingWallThisFrame;

    [Header("References")]
    [Tooltip("Place this Transform at the character's hand/sword tip in the scene.")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private ArcMeleeHitbox meleeHitbox;

    [Header("Attack Data")]
    [Tooltip("Drag your ArcMeleeHitboxData ScriptableObject asset here.")]
    [SerializeField] private ArcMeleeHitboxData attackData;

    [Header("Hit Boxes (Debug Visualization)")]
    [SerializeField] private bool showHeroHitbox = true;
    [SerializeField] private bool showGroundCheckGizmo = true;
    [SerializeField] private bool showAttackHitboxes = true;
    [SerializeField] private bool showEnemyHitboxes = true;

    [Header("Hero Other")]
    public HeroControllerStates cState;

    [Header("Animation Config")]
    [SerializeField] private Animator anim;
    [SerializeField] private string animGrounded = "isGrounded";
    [SerializeField] private string animXVelocity = "xVelocity";
    [SerializeField] private string animYVelocity = "yVelocity";
    [SerializeField] private string animDashing = "isDashing";
    [SerializeField] private string animAttack = "Attack";
    [SerializeField] private string animAttackDir = "AttackDir";
    [SerializeField] private string animJump = "Jump";
    [SerializeField] private string animDoubleJump = "DoubleJump";
    [SerializeField] private string animWallSlide = "isWallSliding";

    private int hashGrounded;
    private int hashXVelocity;
    private int hashYVelocity;
    private int hashDashing;
    private int hashAttack;
    private int hashAttackDir;
    private int hashJump;
    private int hashDoubleJump;
    private int hashWallSlide;

    [Header("Look Config")]
    [SerializeField] private float lookDelay = 0.5f;
    private float lookTimer = 0f;

    [Header("Ground Check Config")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1.2f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private Collider2D selfCollider;
    private readonly Collider2D[] groundHits = new Collider2D[4];

    private float timeSinceLastAttack;
    private float attackCooldownTimer;

    private Rigidbody2D rb2d;
    public ActorStates hero_state;
    public ActorStates prev_hero_state;

    [SerializeField] private float move_input;
    private readonly List<DecayingVelocity> extraAirMoveVelocities = new List<DecayingVelocity>();

    // --- Properties ---
    public bool ShowRuntimeHitbox { get => showHeroHitbox; set => showHeroHitbox = value; }
    public int FacingDirection { get; private set; } = 1;
    public Vector2 AttackOriginPosition => attackOrigin != null
        ? (Vector2)attackOrigin.position
        : (Vector2)transform.position;
    public Rigidbody2D Body => rb2d;

    // -------------------------------------------------------
    // Jump state — replaces the old JUMP_STEPS system
    // -------------------------------------------------------
    private bool isJumping;           // true from jump initiation until apex or button release
    private bool jumpButtonHeld;      // tracks whether space is still held this jump

    // -------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        rb2d = GetComponent<Rigidbody2D>();
        selfCollider = GetComponent<Collider2D>();

        // CRITICAL: disable Unity's built-in gravity so we control it fully
        rb2d.gravityScale = 0f;

        if (cState == null) cState = new HeroControllerStates();

        if (anim == null) anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        hashGrounded = Animator.StringToHash(animGrounded);
        hashXVelocity = Animator.StringToHash(animXVelocity);
        hashYVelocity = Animator.StringToHash(animYVelocity);
        hashDashing = Animator.StringToHash(animDashing);
        hashAttack = Animator.StringToHash(animAttack);
        hashAttackDir = Animator.StringToHash(animAttackDir);
        hashJump = Animator.StringToHash(animJump);
        hashDoubleJump = Animator.StringToHash(animDoubleJump);
        hashWallSlide = Animator.StringToHash(animWallSlide);

        JUMPS_LEFT = 0;
    }

    private bool jumpQueued;
    private bool dashQueued;
    private float dashCooldownTimer;
    private float dashDurationTimer;
    private float jumpCooldownTimer;
    public int JUMPS_LEFT;

    private void Update()
    {
        move_input = ReadMoveInput();

        if (!Mathf.Approximately(move_input, 0f))
            PlayRunningSound();
        else
            StopRunningSound();

        float vInput = ReadVerticalInput();
        bool canLook = Math.Abs(move_input) < 0.1f && cState.onGround;

        if (canLook && Math.Abs(vInput) > 0.1f)
            lookTimer += Time.deltaTime;
        else
        {
            lookTimer = 0f;
            cState.lookingUp = false;
            cState.lookingDown = false;
        }

        if (lookTimer >= lookDelay)
        {
            cState.lookingUp = vInput > 0.1f;
            cState.lookingDown = vInput < -0.1f;
        }

        if (ReadJumpInput()) jumpBufferTimer = JUMP_BUFFER_TIME;
        if (ReadDashInput()) dashQueued = true;

        // Track whether the jump button is still held (for variable height)
        if (!ReadJumpInputHeld()) jumpButtonHeld = false;

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashDurationTimer > 0) dashDurationTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        if (wallJumpLockoutTimer > 0) wallJumpLockoutTimer -= Time.deltaTime;

        timeSinceLastAttack += Time.deltaTime;
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        if (ReadAttackInput()) DoAttack();

        UpdateAnimations();
    }

    private void PlayRunningSound()
    {
        if (audioManager == null) return;
        if (!audioManager.runningSFXSource.isPlaying)
            audioManager.runningSFXSource.Play();
    }

    private void StopRunningSound()
    {
        if (audioManager == null) return;
        if (audioManager.runningSFXSource.isPlaying)
            audioManager.runningSFXSource.Stop();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;
        anim.SetBool(hashGrounded, cState.onGround);
        anim.SetFloat(hashXVelocity, Mathf.Abs(rb2d.linearVelocity.x));
        anim.SetFloat(hashYVelocity, rb2d.linearVelocity.y);
        anim.SetBool(hashDashing, cState.dashing);
        anim.SetBool(hashWallSlide, cState.wallSliding);
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckWall();

        // --- Coyote time ---
        if (cState.onGround)
            coyoteTimer = COYOTE_TIME;
        else
            coyoteTimer -= Time.fixedDeltaTime;

        if (cState.onGround && !wasOnGround) ResetMoveState();
        wasOnGround = cState.onGround;

        // --- Jump input (with buffer + coyote) ---
        bool wantsJump = jumpBufferTimer > 0f;
        if (wantsJump)
        {
            if (cState.wallSliding)
            {
                WallJump();
                jumpBufferTimer = 0f;
            }
            else if ((cState.onGround || coyoteTimer > 0f) && jumpCooldownTimer <= 0f)
            {
                Jump();
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
            }
            else if (!cState.onGround && JUMPS_LEFT > 0 && canDoubleJump)
            {
                DoubleJump();
                jumpBufferTimer = 0f;
            }
        }

        // --- Variable jump cut ---
        if (isJumping && !jumpButtonHeld && rb2d.linearVelocity.y > 0f)
        {
            Vector2 v = rb2d.linearVelocity;
            v.y *= JUMP_CUT_MULTIPLIER;
            rb2d.linearVelocity = v;
            isJumping = false;
        }

        // Clear jumping flag at apex
        if (isJumping && rb2d.linearVelocity.y <= 0f)
            isJumping = false;

        // --- Wall slide ---
        if (touchingWallThisFrame && !cState.onGround && rb2d.linearVelocity.y <= 0f
            && wallJumpLockoutTimer <= 0f)
        {
            // Player must hold input toward the wall
            bool holdingTowardWall = (wallDirection == 1 && move_input > 0.1f)
                                  || (wallDirection == -1 && move_input < -0.1f);
            if (holdingTowardWall)
            {
                if (!cState.wallSliding)
                {
                    cState.wallSliding = true;
                    cState.touchingWall = true;
                    SetState(ActorStates.wall_sliding);
                    // Reset double jump when grabbing a wall (Hollow Knight style)
                    doubleJumped = false;
                    JUMPS_LEFT = 1;
                }
                // Smoothly decelerate to wall slide speed
                Vector2 v = rb2d.linearVelocity;
                if (v.y < -WALLSLIDE_SPEED)
                    v.y = Mathf.MoveTowards(v.y, -WALLSLIDE_SPEED,
                        WALLSLIDE_ACCEL * Mathf.Abs(Physics2D.gravity.y) * Time.fixedDeltaTime);
                rb2d.linearVelocity = v;
            }
            else
            {
                EndWallSlide();
            }
        }
        else
        {
            if (cState.wallSliding)
                EndWallSlide();
        }

        // --- Dash ---
        if (dashQueued && !cState.dashing && dashCooldownTimer <= 0)
        {
            if (cState.onGround || (!cState.onGround && !airDashed))
            {
                if (cState.wallSliding) EndWallSlide();
                Dash();
            }
            dashQueued = false;
        }

        // --- Dash movement ---
        if (cState.dashing)
        {
            if (dashDurationTimer <= 0)
                EndDash();
            else
                rb2d.linearVelocity = new Vector2(FacingDirection * DASH_SPEED, 0f);
        }
        else
        {
            Move(move_input, true);
            if (!cState.wallSliding)
                ApplyCustomGravity();
        }

        // --- Clamp fall speed ---
        ClampFallVelocity();
    }

    // -------------------------------------------------------
    // Custom Gravity  ← THE FIX
    // -------------------------------------------------------

    private void ApplyCustomGravity()
    {
        // No gravity needed while grounded
        if (cState.onGround && rb2d.linearVelocity.y <= 0f) return;

        float gravityToUse;

        if (rb2d.linearVelocity.y > 0f && isJumping && jumpButtonHeld)
        {
            // Ascending with button held → lighter gravity (floaty apex)
            gravityToUse = AIR_HANG_GRAVITY;
        }
        else if (rb2d.linearVelocity.y < 0f)
        {
            // Falling → heavier gravity (snappier landing feel)
            gravityToUse = DEFAULT_GRAVITY * FALL_GRAVITY_MULTIPLIER;
        }
        else
        {
            // Ascending but button released, or at apex
            gravityToUse = DEFAULT_GRAVITY;
        }

        // Apply as a downward acceleration (units/s² scaled by Physics2D gravity magnitude)
        rb2d.linearVelocity += Vector2.down * gravityToUse * Mathf.Abs(Physics2D.gravity.y) * Time.fixedDeltaTime;
    }

    private void ClampFallVelocity()
    {
        Vector2 vel = rb2d.linearVelocity;
        float cap = isDoubleJumping ? MAX_FALL_VELOCITY_DJUMP : MAX_FALL_VELOCITY;
        if (vel.y < -cap)
        {
            vel.y = -cap;
            rb2d.linearVelocity = vel;
        }
    }

    // -------------------------------------------------------
    // Ground Check
    // -------------------------------------------------------

    private void CheckGround()
    {
        Vector2 checkPos;
        if (groundCheck != null)
            checkPos = groundCheck.position;
        else if (selfCollider != null)
        {
            Bounds bounds = selfCollider.bounds;
            checkPos = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
        }
        else
            checkPos = (Vector2)transform.position + new Vector2(0f, -0.51f);

        bool previousGround = cState.onGround;

        int hitCount = Physics2D.OverlapBoxNonAlloc(checkPos, groundCheckSize, 0f, groundHits, groundLayer);

        bool hitGround = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (groundHits[i] != null && groundHits[i] != selfCollider)
            {
                hitGround = true;
                break;
            }
        }

        cState.onGround = hitGround;
        if (cState.onGround != previousGround)
            Debug.Log($"Ground state changed: {cState.onGround}");
    }

    // -------------------------------------------------------
    // Movement
    // -------------------------------------------------------

    private void Move(float moveDirection, bool useInput)
    {
        UpdateGroundState();
        moveDirection = ApplyMovementBlocking(moveDirection);

        Vector2 velocity = rb2d.linearVelocity;

        // During wall-jump lockout, preserve the kickoff velocity
        if (wallJumpLockoutTimer > 0f)
        {
            // Intentionally skip horizontal override
        }
        else if (useInput && !cState.wallSliding)
        {
            float targetSpeed = moveDirection * GetCurrentSpeed();
            float accelRate;

            if (cState.onGround)
                accelRate = (Mathf.Abs(moveDirection) > 0.01f) ? GROUND_ACCEL : GROUND_DECEL;
            else
                accelRate = (Mathf.Abs(moveDirection) > 0.01f) ? AIR_ACCEL : AIR_DECEL;

            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        }

        velocity = ApplyExtraVelocities(velocity);
        rb2d.linearVelocity = velocity;

        // Don't flip facing during wall jump lockout
        if (wallJumpLockoutTimer <= 0f)
        {
            if (moveDirection > 0.01f) FacingDirection = 1;
            else if (moveDirection < -0.01f) FacingDirection = -1;
        }

        TrySetCorrectFacing();
    }

    public void TrySetCorrectFacing()
    {
        bool expectedFacingRight = (FacingDirection == 1);
        if (cState.facingRight != expectedFacingRight)
        {
            cState.facingRight = expectedFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x = cState.facingRight ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }

    private void SetState(ActorStates newState)
    {
        prev_hero_state = hero_state;
        hero_state = newState;
    }

    private void ResetMoveState()
    {
        cState.doubleJumping = false;
        doubleJumped = false;
        airDashed = false;
        isJumping = false;
        isDoubleJumping = false;
        cState.jumping = false;
        cState.wallSliding = false;
        cState.touchingWall = false;
        wallJumpLockoutTimer = 0f;
        JUMPS_LEFT = 1;
    }

    private void UpdateGroundState()
    {
        if (cState.onGround)
            SetState(ActorStates.grounded);
    }

    // -------------------------------------------------------
    // Attack
    // -------------------------------------------------------

    public enum AttackDirection { normal, upward, downward }

    private void DoAttack()
    {
        float cooldown = attackData != null ? attackData.attackCooldown : 0.25f;
        if (attackCooldownTimer > 0f) return;

        if (cState.dashing) EndDash();

        float vertical = ReadVerticalInput();

        if (vertical > 0.1f)
            Attack(AttackDirection.upward);
        else if (vertical < -0.1f && !cState.onGround)
            Attack(AttackDirection.downward);
        else
            Attack(AttackDirection.normal);
    }

    private void Attack(AttackDirection attackDir)
    {
        cState.attacking = true;
        Debug.Log($"[HeroController] Player started attacking! Direction: {attackDir}");

        if (anim != null)
        {
            anim.SetInteger(hashAttackDir, (int)attackDir);
            anim.SetTrigger(hashAttack);
        }

        TrySetCorrectFacing();

        float comboWindow = attackData != null ? attackData.comboTimeWindow : 0.5f;
        if (timeSinceLastAttack <= comboWindow)
            cState.altAttack = !cState.altAttack;
        else
            cState.altAttack = false;

        if (cState.wallSliding)
        {
            if (attackDir == AttackDirection.normal)
            {
                Vector2 v = rb2d.linearVelocity;
                v.y *= 0.5f;
                rb2d.linearVelocity = v;
            }
            else
                cState.wallSliding = false;
        }

        if (meleeHitbox != null)
            meleeHitbox.PerformAttack(attackDir, cState.facingRight);

        DidAttack();
    }

    private void DidAttack()
    {
        timeSinceLastAttack = 0f;
        float cooldown = attackData != null ? attackData.attackCooldown : 0.25f;
        attackCooldownTimer = cooldown;
        Invoke(nameof(EndAttack), 0.15f);

        audioManager.PlaySFX(audioManager.fighting);
    }

    private void EndAttack()
    {
        cState.attacking = false;
        Debug.Log("[HeroController] Player stopped attacking.");
    }

    // -------------------------------------------------------
    // Movement Helpers
    // -------------------------------------------------------

    private float ApplyMovementBlocking(float moveDirection)
    {
        if (IsInSpikeRecovery()) return 0f;
        if (IsBlockedBySlopeLeft(moveDirection)) return 0f;
        if (IsBlockedBySlopeRight(moveDirection)) return 0f;
        return moveDirection;
    }

    private bool IsInSpikeRecovery() => cState.downSpikeRecovery && cState.onGround;
    private bool IsBlockedBySlopeLeft(float d) => cState.isTouchingSlopeLeft && d < 0f;
    private bool IsBlockedBySlopeRight(float d) => cState.isTouchingSlopeRight && d > 0f;
    private float GetCurrentSpeed() => (cState.inWalkZone && cState.onGround) ? WALK_SPEED : RUN_SPEED;

    public void AddExtraAirMoveVelocity(DecayingVelocity velocity) =>
        extraAirMoveVelocities.Add(velocity);

    private Vector2 ApplyExtraVelocities(Vector2 velocity)
    {
        foreach (var dv in extraAirMoveVelocities)
            if (!ShouldSkipVelocity(dv))
                velocity += dv.Velocity;
        return velocity;
    }

    private bool ShouldSkipVelocity(DecayingVelocity dv)
    {
        return dv.SkipBehaviour switch
        {
            DecayingVelocity.SkipBehaviours.None => false,
            DecayingVelocity.SkipBehaviours.WhileMoving => IsMoving(),
            DecayingVelocity.SkipBehaviours.WhileMovingForward => IsMovingForward(),
            DecayingVelocity.SkipBehaviours.WhileMovingBackward => IsMovingBackward(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool IsMoving() => Math.Abs(move_input) > Mathf.Epsilon;
    private bool IsMovingForward() => cState.facingRight ? move_input > Mathf.Epsilon : move_input < Mathf.Epsilon;
    private bool IsMovingBackward() => cState.facingRight ? move_input < -Mathf.Epsilon : move_input > Mathf.Epsilon;

    // -------------------------------------------------------
    // Actions
    // -------------------------------------------------------

    public void Jump()
    {
        isJumping = true;
        isDoubleJumping = false;
        jumpButtonHeld = true;
        cState.jumping = true;
        jumpCooldownTimer = 0.2f;

        if (anim != null) anim.SetTrigger(hashJump);

        // Single impulse — gravity handles the arc from here
        Vector2 v = rb2d.linearVelocity;
        v.y = JUMP_SPEED;
        rb2d.linearVelocity = v;

        audioManager.PlaySFX(audioManager.jumping);
    }

    public void DoubleJump()
    {
        isJumping = true;
        isDoubleJumping = true;
        jumpButtonHeld = true;
        cState.doubleJumping = true;
        doubleJumped = true;
        JUMPS_LEFT--;

        if (anim != null) anim.SetTrigger(hashDoubleJump);

        Vector2 v = rb2d.linearVelocity;
        v.y = doubleJumpSpeed;
        rb2d.linearVelocity = v;
    }

    public void Dash()
    {
        cState.dashing = true;
        dashDurationTimer = DASH_DISTANCE / DASH_SPEED;
        dashCooldownTimer = DASH_COOLDOWN;
        if (!cState.onGround) airDashed = true;

        Vector2 v = rb2d.linearVelocity;
        v.y = 0f;
        v.x = FacingDirection * DASH_SPEED;
        rb2d.linearVelocity = v;

        audioManager.SFXSource.PlayOneShot(audioManager.dashing, 0.2f);
    }

    private void EndDash()
    {
        cState.dashing = false;
        rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
    }

    // -------------------------------------------------------
    // Wall Detection & Wall Jump
    // -------------------------------------------------------

    private void CheckWall()
    {
        touchingWallThisFrame = false;
        wallDirection = 0;

        if (cState.onGround || cState.dashing) return;

        // Cast rays from collider center to detect walls
        Vector2 origin = selfCollider != null
            ? (Vector2)selfCollider.bounds.center
            : (Vector2)transform.position;

        // Check right side
        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, WALLJUMP_RAY_LENGTH, groundLayer);
        if (hitRight.collider != null && hitRight.collider != selfCollider)
        {
            touchingWallThisFrame = true;
            wallDirection = 1;
            return;
        }

        // Check left side
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, WALLJUMP_RAY_LENGTH, groundLayer);
        if (hitLeft.collider != null && hitLeft.collider != selfCollider)
        {
            touchingWallThisFrame = true;
            wallDirection = -1;
        }
    }

    public void WallJump()
    {
        cState.wallSliding = false;
        cState.touchingWall = false;
        isJumping = true;
        isDoubleJumping = false;
        jumpButtonHeld = true;
        cState.jumping = true;
        jumpCooldownTimer = 0.2f;

        // Brief lockout so the kickoff arc feels natural
        wallJumpLockoutTimer = WJ_INPUT_LOCKOUT;

        if (anim != null) anim.SetTrigger(hashJump);

        // Kick away from wall + upward impulse
        float kickDirection = -wallDirection;
        Vector2 v = rb2d.linearVelocity;
        v.x = kickDirection * WJ_KICKOFF_SPEED;
        v.y = WJ_JUMP_SPEED;
        rb2d.linearVelocity = v;

        // Face away from wall
        FacingDirection = (int)kickDirection;
        TrySetCorrectFacing();

        SetState(ActorStates.airborne);
    }

    private void EndWallSlide()
    {
        cState.wallSliding = false;
        cState.touchingWall = false;
    }

    // -------------------------------------------------------
    // Debug Gizmos
    // -------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (showGroundCheckGizmo)
        {
            Vector2 checkPos;
            if (groundCheck != null) checkPos = groundCheck.position;
            else if (TryGetComponent<Collider2D>(out var col))
            {
                Bounds bounds = col.bounds;
                checkPos = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
            }
            else checkPos = (Vector2)transform.position + new Vector2(0f, -0.51f);

            bool grounded = cState != null && cState.onGround;
            Gizmos.color = grounded ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(checkPos, groundCheckSize);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(checkPos, groundCheckSize);
        }

        if (showHeroHitbox && TryGetComponent<Collider2D>(out var myCol))
        {
            Gizmos.color = Color.yellow;
            DrawColliderGizmo(myCol);
        }

        if (showAttackHitboxes || showEnemyHitboxes)
        {
            Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
            foreach (var col in allColliders)
            {
                if (col.gameObject == this.gameObject) continue;

                string objName = col.gameObject.name.ToLower();
                int layer = col.gameObject.layer;
                bool isAttack = objName.Contains("slash") || objName.Contains("attack") || layer == LayerMask.NameToLayer("Attack");
                bool isEnemy = objName.Contains("enemy") || objName.Contains("boss") || layer == LayerMask.NameToLayer("Enemy");

                if (isAttack && showAttackHitboxes)
                {
                    Gizmos.color = Color.red;
                    DrawColliderGizmo(col);
                }
                else if (isEnemy && showEnemyHitboxes)
                {
                    Gizmos.color = Color.magenta;
                    DrawColliderGizmo(col);
                }
            }
        }

        if (attackOrigin != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackOrigin.position, 0.1f);
        }
    }

    private void DrawColliderGizmo(Collider2D col)
    {
        if (col is BoxCollider2D box)
        {
            Gizmos.matrix = col.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else if (col is CircleCollider2D circle)
        {
            Gizmos.matrix = col.transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(circle.offset, circle.radius);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }

    // -------------------------------------------------------
    // State Accessors
    // -------------------------------------------------------

    public bool GetState(string stateName) => cState.GetState(stateName);
    public bool GetCState(string stateName) => cState.GetState(stateName);
    public void SetCState(string stateName, bool v) => cState.SetState(stateName, v);
    public static bool CStateExists(string stateName) => HeroControllerStates.CStateExists(stateName);

    // -------------------------------------------------------
    // Input
    // -------------------------------------------------------

    private float ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float h = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            return h;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxisRaw("Horizontal");
#else
        return 0f;
#endif
    }

    private bool ReadAttackInput()
    {
#if ENABLE_INPUT_SYSTEM
        bool inputSysZ = Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame;
        bool legacyZ = false;
        try { legacyZ = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Z); } catch { }
        return inputSysZ || legacyZ;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Z);
#else
        return Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Fire1");
#endif
    }

    private float ReadVerticalInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float v = 0f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            return v;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxisRaw("Vertical");
#else
        return 0f;
#endif
    }

    private bool ReadJumpInput()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump");
#else
        return false;
#endif
    }

    private bool ReadJumpInputHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButton("Jump");
#else
        return false;
#endif
    }

    private bool ReadDashInput()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                Keyboard.current.cKey.wasPressedThisFrame ||
                Keyboard.current.kKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.K);
#else
        return false;
#endif
    }

    // -------------------------------------------------------
    // Inner Types
    // -------------------------------------------------------

    [Serializable]
    public struct DecayingVelocity
    {
        public Vector2 Velocity;
        public SkipBehaviours SkipBehaviour;

        public enum SkipBehaviours
        {
            None,
            WhileMoving,
            WhileMovingForward,
            WhileMovingBackward
        }
    }

    private class BoolFieldAccessOptimizer<T> { }
}