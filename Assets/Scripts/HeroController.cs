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

    // A better approach matching the actual game is using collider bounds, but this matches the placeholder
    private void CheckGround()
    {
        cState.onGround = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
        cState.wasOnGround = cState.onGround;
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
        if (cState == null)
            cState = new HeroControllerStates();
        // deifne state so u cant inf jump (next week)
    }

    private void Update()
    {
        move_input = ReadMoveInput();
        if (!Mathf.Approximately(move_input, 0f))
            Move(move_input, true);

        if (ReadJumpInput() && cState.onGround)
            Jump();


        // Store physics and store input (next week)
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move(move_input, false);
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

    // --- State ---

    private void SetState(ActorStates newState)
    {
        prev_hero_state = hero_state;
        hero_state = newState;
    }

    // --- Ground ---

    private void UpdateGroundState()
    {
        if (cState.onGround)
            SetState(ActorStates.grounded);
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

    private float GetWalkSpeed() => 3f; // TODO: replace with real value
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
        if (jump_steps <= JUMP_STEPS)
        {
            Vector2 velocity = rb2d.linearVelocity;
            velocity.y = JUMP_SPEED;
            rb2d.linearVelocity = velocity;
            jump_steps++;
        }
        else
        {
            cState.jumping = false;
        }
    }

    public void DoubleJump()
    {
        if (doubleJump_steps <= DOUBLE_JUMP_RISE_STEPS + DOUBLE_JUMP_FALL_STEPS)
        {
            if (doubleJump_steps > DOUBLE_JUMP_FALL_STEPS)
            {
                rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, JUMP_SPEED * 1.1f);
            }
            doubleJump_steps++;
        }
        else
        {
            cState.doubleJumping = false;
        }
        if (cState.onGround)
        {
            cState.doubleJumping = false;
        }
    }

    // -------------------------------------------------------
    // Physics
    // -------------------------------------------------------

    /*
    public void AffectedByGravity(bool gravityApplies)
    {
        if (gravityApplies && this.CheckAndRequestUnlock(HeroLockStates.GravityLocked))
        {
            return;
        }
        this.RemoveUnlockRequest(HeroLockStates.GravityLocked);
        this.IsGravityApplied = gravityApplies;
        if (this.rb2d.gravityScale > Mathf.Epsilon && !gravityApplies)
        {
            this.prevGravityScale = this.rb2d.gravityScale;
            this.rb2d.gravityScale = 0f;
            return;
        }
        if (this.rb2d.gravityScale <= Mathf.Epsilon && gravityApplies)
        {
            this.rb2d.gravityScale = this.prevGravityScale;
            this.prevGravityScale = 0f;
        }
    }
    */

    public bool GetState(string stateName)
    {
        return this.cState.GetState(stateName);
    }

    // -------------------------------------------------------
    // states
    // -------------------------------------------------------
    public bool GetCState(string stateName)
    {
        return this.cState.GetState(stateName);
    }

    public void SetCState(string stateName, bool value)
    {
        this.cState.SetState(stateName, value);
    }

    public static bool CStateExists(string stateName)
    {
        return HeroControllerStates.CStateExists(stateName);
    }

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
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
    return Input.GetButtonDown("Jump");
#else
    return false;
#endif
    }

    // -------------------------------------------------------
    // Inner Types
    // -------------------------------------------------------

    /*
    [Serializable]
    public class HeroControllerStates
    {
        public bool facingRight;
        public bool onGround;
        public bool wallSliding;
        public bool inWalkZone;
        public bool downSpikeRecovery;
        public bool isTouchingSlopeLeft;
        public bool isTouchingSlopeRight;
        public bool invulnerable;
        public int invulnerableCount;

        private static BoolFieldAccessOptimizer<HeroControllerStates> boolFieldAccessOptimizer;

        public bool Invulnerable => invulnerable || invulnerableCount > 0;

        public HeroControllerStates()
        {
            facingRight = false;
            if (boolFieldAccessOptimizer == null)
                boolFieldAccessOptimizer = new BoolFieldAccessOptimizer<HeroControllerStates>();
            Reset();
        }

        public void Reset()
        {
            onGround = false;
            wallSliding = false;
            inWalkZone = false;
            downSpikeRecovery = false;
            isTouchingSlopeLeft = false;
            isTouchingSlopeRight = false;
            invulnerable = false;
            invulnerableCount = 0;
        }
    }
    */

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