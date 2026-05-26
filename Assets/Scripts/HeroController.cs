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
    [SerializeField] private int JUMP_STEPS = 16;
    [SerializeField] private int JUMP_STEPS_MIN = 4;

    [Header("Movement - Double Jump")]
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private float doubleJumpSpeed = 18f;

    [Header("Movement - Dash")]
    [SerializeField] private float DASH_SPEED = 20f;
    [SerializeField] private float DASH_DISTANCE = 6f; 
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

    [Header("Attack Settings")]
    [SerializeField] private float comboTimeWindow = 0.5f;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private GameObject normalSlash;
    [SerializeField] private GameObject alternateSlash;
    [SerializeField] private GameObject upSlash;
    [SerializeField] private GameObject altUpSlash;
    [SerializeField] private GameObject downSlash;
    [SerializeField] private GameObject altDownSlash;
    [SerializeField] private GameObject wallSlash;

    private float timeSinceLastAttack;
    private float attackCooldownTimer;

    [Header("Hit Boxes (Debug Visualization)")]
    [SerializeField] private bool showHeroHitbox = true;
    [SerializeField] private bool showGroundCheckGizmo = true;
    [SerializeField] private bool showAttackHitboxes = true;
    [SerializeField] private bool showEnemyHitboxes = true;

    [Header("Hero Other")]
    public HeroControllerStates cState;

    [Header("Look Config")]
    [SerializeField] private float lookDelay = 0.5f;
    private float lookTimer = 0f;

    [Header("Ground Check Config")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1.2f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

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

            checkPos = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
        }
        else
        {
            checkPos = (Vector2)transform.position + new Vector2(0f, -0.51f);
        }

        bool previousGround = cState.onGround;

        int hitCount = Physics2D.OverlapBoxNonAlloc(
            checkPos,
            groundCheckSize,
            0f,
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

    // FIX 3: Dynamic Hit Box Visualization
    private void OnDrawGizmos()
    {
        // 1. Ground Check Visualization
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

        // 2. Hero Hitbox Visualization
        if (showHeroHitbox && TryGetComponent<Collider2D>(out var myCol))
        {
            Gizmos.color = Color.yellow;
            DrawColliderGizmo(myCol);
        }

        // 3. Dynamic Attacks and Enemies Visualization
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
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }

    private Rigidbody2D rb2d;

    public ActorStates hero_state;
    public ActorStates prev_hero_state;

    [SerializeField] private float move_input;
    private readonly List<DecayingVelocity> extraAirMoveVelocities = new List<DecayingVelocity>();

    // --- Properties ---
    public bool ShowRuntimeHitbox { get => showHeroHitbox; set => showHeroHitbox = value; }
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

        // Update looking states for camera
        float vInput = ReadVerticalInput();
        bool canLook = Math.Abs(move_input) < 0.1f && cState.onGround;

        if (canLook && Math.Abs(vInput) > 0.1f)
        {
            lookTimer += Time.deltaTime;
        }
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

        if (ReadJumpInput()) jumpQueued = true;
        if (ReadDashInput()) dashQueued = true;

        // Timers are fine in Update — they don't touch the Rigidbody.
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashDurationTimer > 0) dashDurationTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        timeSinceLastAttack += Time.deltaTime;
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        if (ReadAttackInput()) DoAttack();
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
            else if (!cState.onGround && JUMPS_LEFT > 0 && canDoubleJump)
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

        // --- Variable Jump Logic ---
        if (cState.jumping && !cState.onGround)
        {
            jump_steps++;

            // Force jump upward duration for at least JUMP_STEPS_MIN frames even if released early, 
            // otherwise continue while held up to JUMP_STEPS max.
            if (ReadJumpInputHeld() || jump_steps < JUMP_STEPS_MIN)
            {
                if (jump_steps <= JUMP_STEPS)
                {
                    Vector2 v = rb2d.linearVelocity;
                    v.y = JUMP_SPEED;
                    rb2d.linearVelocity = v;
                }
                else
                {
                    CancelJump();
                }
            }
            else
            {
                JumpReleased();
            }
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

        TrySetCorrectFacing();
    }

    // --- Facing ---

    public void TrySetCorrectFacing()
    {
        bool expectedFacingRight = (FacingDirection == 1);

        if (cState.facingRight != expectedFacingRight)
        {
            cState.facingRight = expectedFacingRight;


            Vector3 localScale = transform.localScale;
            // Inverted the absolute value logic to fix moonwalking
            // (Used when the base character sprite naturally faces the opposite direction)
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
    //-------------------------------------------
    //              Attack
    //-------------------------------------------
    public enum AttackDirection { normal, upward, downward }

    private void DoAttack()
    {
        if (attackCooldownTimer > 0f) return;

        // Cancel dashes if executing an attack
        if (cState.dashing)
        {
            EndDash();
        }

        float vertical = ReadVerticalInput();

        if (vertical > 0.1f)
        {
            Attack(AttackDirection.upward);
        }
        else if (vertical < -0.1f && !cState.onGround)
        {
            // Only downward strike if in the air
            Attack(AttackDirection.downward);
        }
        else
        {
            Attack(AttackDirection.normal);
        }
    }

    private void Attack(AttackDirection attackDir)
    {
        cState.attacking = true;
        Debug.Log($"[HeroController] Player started attacking! Direction: {attackDir}");

        TrySetCorrectFacing();

        // Alternate Slash System (Combos)
        if (timeSinceLastAttack <= comboTimeWindow)
        {
            cState.altAttack = !cState.altAttack;
        }
        else
        {
            cState.altAttack = false;
        }

        GameObject selectedSlash = normalSlash;
        float angle = FacingDirection == 1 ? 0f : 180f;

        // Wall-Sliding Interruption
        if (cState.wallSliding)
        {
            if (attackDir == AttackDirection.normal)
            {
                // Slow down descent temporarily for wall slash
                Vector2 v = rb2d.linearVelocity;
                v.y *= 0.5f;
                rb2d.linearVelocity = v;
                selectedSlash = wallSlash;
            }
            else
            {
                // Up and Down attacks force you to immediately release from the wall
                cState.wallSliding = false;
            }
        }

        // Select correct hitbox prefab based on direction and combo
        if (!cState.wallSliding || attackDir != AttackDirection.normal)
        {
            if (attackDir == AttackDirection.upward)
            {
                selectedSlash = cState.altAttack ? altUpSlash : upSlash;
                angle = 90f; // Attack upwards
            }
            else if (attackDir == AttackDirection.downward)
            {
                selectedSlash = cState.altAttack ? altDownSlash : downSlash;
                angle = 270f; // Attack downwards
            }
            else
            {
                selectedSlash = cState.altAttack ? alternateSlash : normalSlash;
            }
        }

        // Spawn hitboxes
        if (selectedSlash != null)
        {
            GameObject slashInst = Instantiate(selectedSlash, AttackOriginPosition, Quaternion.Euler(0, 0, angle));
            Destroy(slashInst, 0.15f); // Automatically cleanup the spawned hitbox/effect after 0.15s
        }

        DidAttack();
    }

    private void DidAttack()
    {
        timeSinceLastAttack = 0f;
        attackCooldownTimer = attackCooldown;
        Invoke(nameof(EndAttack), 0.15f);
    }

    private void EndAttack()
    {
        cState.attacking = false;
        Debug.Log("[HeroController] Player stopped attacking.");
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

        Vector2 v = rb2d.linearVelocity;
        v.y = JUMP_SPEED;
        rb2d.linearVelocity = v;
    }

    private void JumpReleased()
    {
        if (cState.jumping)
        {
            // If the minimum jump frames have passed and we are still moving up, instantly stunt vertical speed to reward the short hop
            if (jump_steps >= JUMP_STEPS_MIN && rb2d.linearVelocity.y > 0)
            {
                Vector2 v = rb2d.linearVelocity;
                v.y *= 0.5f;
                rb2d.linearVelocity = v;
            }
            CancelJump();
        }
    }

    private void CancelJump()
    {
        cState.jumping = false;
    }
    //public bool TrySetCorrectFacing(bool force = false)
    //{
    //    if (!this.CanTurn && !force)
    //    {
    //        return false;
    //    }
    //    if (this.move_input > 0f && !this.cState.facingRight)
    //    {
    //        this.FlipSprite();
    //        return true;
    //    }
    //    if (this.move_input < 0f && this.cState.facingRight)
    //    {
    //        this.FlipSprite();
    //        return true;
    //    }
    //    return false;
    //}

    public void DoubleJump()
    {
        cState.doubleJumping = true;
        doubleJumped = true;
        JUMPS_LEFT--;

        Vector2 v = rb2d.linearVelocity;
        v.y = doubleJumpSpeed;
        rb2d.linearVelocity = v;
    }

    public void Dash()
    {
        cState.dashing = true;
        // Time = Distance / Speed. This allows the dash to perfectly reflect the DASH_SPEED and DASH_DISTANCE in the inspector.
        dashDurationTimer = DASH_DISTANCE / DASH_SPEED;
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
            float vertical = 0f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            return vertical;
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
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump");
#else
        return false;
#endif
    }

    private bool ReadJumpInputHeld()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.isPressed;
        }
        return false;
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
               (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.kKey.wasPressedThisFrame);
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