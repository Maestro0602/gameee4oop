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
    [Header("Movement - Run & Walk")]
    [SerializeField] private float RUN_SPEED = 8.3f;
    [SerializeField] private float WALK_SPEED = 6f;

    [Header("Movement - Jump")]
    [SerializeField] private float JUMP_SPEED = 16.5f;
    [SerializeField] private float MIN_JUMP_SPEED = 5f;
    [SerializeField] private int JUMP_STEPS = 16;
    [SerializeField] private int JUMP_STEPS_MIN = 4;
    [SerializeField] private int DOUBLE_JUMP_RISE_STEPS = 5;
    [SerializeField] private int DOUBLE_JUMP_FALL_STEPS = 5;
    [SerializeField] private float JUMP_ABILITY_GROUND_RAY_LENGTH = 0.5f;

    [Header("Movement - Dash")]
    [SerializeField] private float DASH_SPEED = 20f;
    [SerializeField] private float DASH_TIME = 0.3f;
    [SerializeField] private float AIR_DASH_TIME = 0.3f;
    [SerializeField] private float DOWN_DASH_TIME = 0.2f;
    [SerializeField] private int DASH_QUEUE_STEPS = 5;
    [SerializeField] private float DASH_COOLDOWN = 0.6f;

    [Header("Movement - Wall")]
    [SerializeField] private float WALLJUMP_RAY_LENGTH = 1.2f;
    [SerializeField] private float WJ_KICKOFF_SPEED = 10f;
    [SerializeField] private float WALLSLIDE_ACCEL = 2.5f;
    [SerializeField] private float WALLSLIDE_STICK_TIME = 0.2f;
    [SerializeField] private float WALLCLING_DECEL = 0.5f;
    [SerializeField] private float WALLCLING_COOLDOWN = 0.5f;

    [Header("Movement - Gravity")]
    [SerializeField] private float DEFAULT_GRAVITY = 2.5f;
    [SerializeField] private float AIR_HANG_GRAVITY = 1.2f;
    [SerializeField] private float MAX_FALL_VELOCITY = 20f;
    [SerializeField] private float MAX_FALL_VELOCITY_DJUMP = 15f;

    // Internal State Variables
    private int jump_steps;
    private int jumped_steps;
    private int doubleJump_steps;
    private bool doubleJumped;
    private bool airDashed;
    private bool wasOnGround;
    private float dash_timer;
    private float dash_time;

    [Header("References")]
    [SerializeField] private Transform attackOrigin;

    [Header("Optional Attack Data")]
    [SerializeField] private AttackDefinition2D[] groundCombo;
    [SerializeField] private AttackDefinition2D[] airCombo;
    [SerializeField] private AttackDefinition2D specialAttack;

    [Header("Debug")]
    [SerializeField] private bool showRuntimeHitbox;

    [Header("Hero Other")]
    public HeroControllerStates cState;

    [Header("Ground Check Config")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // FIX 1 (Overlap Layer) + FIX 2 (GroundCheck Position):
    //   - Cache the player's own collider and pass a ContactFilter2D that excludes it.
    //   - Add a -0.01f sink so the probe point is just below the collider edge, not flush with it.
    private Collider2D selfCollider;
    private readonly Collider2D[] groundHits = new Collider2D[4];

    private void CheckGround()
    {
        Vector2 checkPos;

        if (groundCheck != null)
        {
            checkPos = groundCheck.position;
        }
        else if (selfCollider != null)
        {
            Bounds bounds = selfCollider.bounds;
            // FIX 2: sink 0.01 units below the collider floor so flush surfaces register
            checkPos = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
        }
        else
        {
            checkPos = (Vector2)transform.position + new Vector2(0f, -0.51f);
        }

        bool previousGround = cState.onGround;

        // FIX 1: Use NonAlloc variant and filter out hits that are our own collider.
        // This prevents the player's own Collider2D from satisfying the ground check.
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            checkPos,
            groundCheckRadius,
            groundHits,
            groundLayer
        );

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

    // FIX 3: Debug visualization — draw the ground probe in the Scene view at all times.
    private void OnDrawGizmos()
    {
        Vector2 checkPos;

        if (groundCheck != null)
        {
            checkPos = groundCheck.position;
        }
        else if (TryGetComponent<Collider2D>(out var col))
        {
            Bounds bounds = col.bounds;
            checkPos = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
        }
        else
        {
            checkPos = (Vector2)transform.position + new Vector2(0f, -0.51f);
        }

        // Green when grounded, red when airborne (cState may be null in edit mode)
        bool grounded = cState != null && cState.onGround;
        Gizmos.color = grounded ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(checkPos, groundCheckRadius);

        // White outline for clarity regardless of state
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
    }

    private Rigidbody2D rb2d;

    public ActorStates hero_state;
    public ActorStates prev_hero_state;

    [SerializeField] private float move_input;
    private readonly List<DecayingVelocity> extraAirMoveVelocities = new List<DecayingVelocity>();

    // --- Properties ---
    public bool ShowRuntimeHitbox { get => showRuntimeHitbox; set => showRuntimeHitbox = value; }
    public int FacingDirection { get; private set; } = 1;
    public Vector2 AttackOriginPosition => attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;
    public AttackDefinition2D[] GroundCombo => groundCombo;
    public AttackDefinition2D[] AirCombo => airCombo;
    public AttackDefinition2D SpecialAttack => specialAttack;
    public Rigidbody2D Body => rb2d;

    // -------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            UnityEngine.Object.Destroy(gameObject);
            return;
        }
        instance = this;
        rb2d = GetComponent<Rigidbody2D>();
        // FIX 1: Cache own collider once so CheckGround can exclude it every frame.
        selfCollider = GetComponent<Collider2D>();
        if (cState == null)
            cState = new HeroControllerStates();

        // FIX 5: JUMPS_LEFT starts at 0 — the player has no midair jumps until they land.
        // ResetMoveState() sets it to 1 on landing, which is the correct grant point.
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
        // FIX 4: Only READ input here. Never call Jump() or DoubleJump() from Update.
        // Physics mutations belong exclusively in FixedUpdate to avoid double-firing
        // on frames where both Update and FixedUpdate run (which is the common case).
        move_input = ReadMoveInput();
        if (ReadJumpInput()) jumpQueued = true;
        if (ReadDashInput()) dashQueued = true;

        // Timers are fine in Update — they don't touch the Rigidbody.
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashDurationTimer > 0) dashDurationTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        CheckGround(); // onGround is authoritative from here down

        if (cState.onGround && !wasOnGround)
            ResetMoveState();
        wasOnGround = cState.onGround;

        // FIX 4: All jump/dash execution lives only in FixedUpdate.
        if (jumpQueued)
        {
            if (cState.onGround && jumpCooldownTimer <= 0f)
                Jump();
            else if (!cState.onGround && JUMPS_LEFT > 0)
                DoubleJump();

            jumpQueued = false;
        }

        if (dashQueued && !cState.dashing && dashCooldownTimer <= 0)
        {
            if (cState.onGround || (!cState.onGround && !airDashed))
                Dash();
            dashQueued = false;
        }

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
        }
    }

    // -------------------------------------------------------
    // Movement
    // -------------------------------------------------------

    private void Move(float moveDirection, bool useInput)
    {
        UpdateGroundState();
        moveDirection = ApplyMovementBlocking(moveDirection);

        Vector2 velocity = rb2d.linearVelocity;

        if (useInput && !cState.wallSliding)
            velocity.x = moveDirection * GetCurrentSpeed();

        velocity = ApplyExtraVelocities(velocity);
        rb2d.linearVelocity = velocity;

        if (moveDirection > 0.01f)
            FacingDirection = 1;
        else if (moveDirection < -0.01f)
            FacingDirection = -1;
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
        cState.jumping = false;
        JUMPS_LEFT = 1;
    }

    private void UpdateGroundState()
    {
        if (cState.onGround)
        {
            SetState(ActorStates.grounded);
            jump_steps = 0;
        }
    }

    // --- Movement Blocking ---

    private float ApplyMovementBlocking(float moveDirection)
    {
        if (IsInSpikeRecovery()) return 0f;
        if (IsBlockedBySlopeLeft(moveDirection)) return 0f;
        if (IsBlockedBySlopeRight(moveDirection)) return 0f;
        return moveDirection;
    }

    private bool IsInSpikeRecovery() => cState.downSpikeRecovery && cState.onGround;
    private bool IsBlockedBySlopeLeft(float moveDirection) => cState.isTouchingSlopeLeft && moveDirection < 0f;
    private bool IsBlockedBySlopeRight(float moveDirection) => cState.isTouchingSlopeRight && moveDirection > 0f;

    // --- Speed ---

    private float GetCurrentSpeed()
    {
        if (cState.inWalkZone && cState.onGround)
            return GetWalkSpeed();
        return GetRunSpeed();
    }

    private float GetWalkSpeed() => WALK_SPEED;
    private float GetRunSpeed() => RUN_SPEED;

    // --- Extra Velocities ---

    public void AddExtraAirMoveVelocity(DecayingVelocity velocity)
    {
        extraAirMoveVelocities.Add(velocity);
    }

    private Vector2 ApplyExtraVelocities(Vector2 velocity)
    {
        foreach (var dv in extraAirMoveVelocities)
        {
            if (!ShouldSkipVelocity(dv))
                velocity += dv.Velocity;
        }
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
    private bool IsMovingBackward() => cState.facingRight ? move_input < Mathf.Epsilon : move_input > Mathf.Epsilon;

    // -------------------------------------------------------
    // Actions
    // -------------------------------------------------------

    public void Jump()
    {
        jump_steps = 0;
        cState.jumping = true;
        jumpCooldownTimer = 0.2f;
        // FIX 5: Removed "JUMPS_LEFT = 1" from here.
        // The double-jump budget is granted by ResetMoveState() on landing, not here.

        Vector2 v = rb2d.linearVelocity;
        v.y = JUMP_SPEED;
        rb2d.linearVelocity = v;
    }

    public void DoubleJump()
    {
        cState.doubleJumping = true;
        doubleJumped = true;
        JUMPS_LEFT--;

        Vector2 v = rb2d.linearVelocity;
        v.y = JUMP_SPEED * 1.1f;
        rb2d.linearVelocity = v;
    }

    public void Dash()
    {
        cState.dashing = true;
        dashDurationTimer = DASH_TIME;
        dashCooldownTimer = DASH_COOLDOWN;

        if (!cState.onGround)
            airDashed = true;

        Vector2 v = rb2d.linearVelocity;
        v.y = 0f;
        v.x = FacingDirection * DASH_SPEED;
        rb2d.linearVelocity = v;
    }

    private void EndDash()
    {
        cState.dashing = false;
        rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
    }

    // -------------------------------------------------------
    // Physics
    // -------------------------------------------------------

    public bool GetState(string stateName)
    {
        return this.cState.GetState(stateName);
    }

    // -------------------------------------------------------
    // States
    // -------------------------------------------------------

    public bool GetCState(string stateName) => this.cState.GetState(stateName);
    public void SetCState(string stateName, bool value) => this.cState.SetState(stateName, value);
    public static bool CStateExists(string stateName) => HeroControllerStates.CStateExists(stateName);

    // -------------------------------------------------------
    // Input
    // -------------------------------------------------------

    private float ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float horizontal = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            return horizontal;
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
        return Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Fire1");
#else
        return false;
#endif
    }

    private bool ReadJumpInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame ||
                   Keyboard.current.upArrowKey.wasPressedThisFrame ||
                   Keyboard.current.wKey.wasPressedThisFrame;
        }
        return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump");
#else
        return false;
#endif
    }

    private bool ReadDashInput()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Fire3");
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