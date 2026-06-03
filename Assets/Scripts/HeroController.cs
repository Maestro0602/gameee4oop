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

    [Header("Movement - Double Jump")]
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private float doubleJumpSpeed = 18f;

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

    [Header("Movement - Gravity")]
    [Tooltip("Gravity applied while falling or at apex with no input.")]
    [SerializeField] private float DEFAULT_GRAVITY = 2.5f;
    [Tooltip("Reduced gravity while ascending (gives floaty apex feel).")]
    [SerializeField] private float AIR_HANG_GRAVITY = 1.2f;
    [Tooltip("How much to multiply gravity when falling fast (snappier landing).")]
    [SerializeField] private float FALL_GRAVITY_MULTIPLIER = 1.5f;
    [SerializeField] private float MAX_FALL_VELOCITY = 20f;
    [SerializeField] private float MAX_FALL_VELOCITY_DJUMP = 15f;
    [Tooltip("How much to cut vertical speed when jump button is released early.")]
    [SerializeField] private float JUMP_CUT_MULTIPLIER = 0.5f;

    // Internal State
    private bool doubleJumped;
    private bool airDashed;
    private bool wasOnGround;
    private bool isDoubleJumping;     // track if last jump was a double jump (affects fall cap)

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

    private int hashGrounded;
    private int hashXVelocity;
    private int hashYVelocity;
    private int hashDashing;
    private int hashAttack;
    private int hashAttackDir;
    private int hashJump;
    private int hashDoubleJump;

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

        JUMPS_LEFT = 0;

        selfCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        if (ReadJumpInput()) jumpQueued = true;
        if (ReadDashInput()) dashQueued = true;

        // Track whether the jump button is still held (for variable height)
        if (!ReadJumpInputHeld()) jumpButtonHeld = false;

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashDurationTimer > 0) dashDurationTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        timeSinceLastAttack += Time.deltaTime;
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        if (ReadAttackInput()) DoAttack();

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;
        bool isMoving = Mathf.Abs(move_input) > 0.05f;

        anim.SetBool("isRunning", isMoving);
        anim.SetBool(hashGrounded, cState.onGround);
        anim.SetFloat(hashYVelocity, rb2d.linearVelocity.y);
        anim.SetBool(hashDashing, cState.dashing);
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (cState.onGround && !wasOnGround) ResetMoveState();
        wasOnGround = cState.onGround;

        // --- Jump input ---
        if (jumpQueued)
        {
            Debug.Log($"jumpQueued fired! onGround={cState.onGround}, cooldown={jumpCooldownTimer}");
            if (cState.onGround && jumpCooldownTimer <= 0f)
                Jump();
            else if (!cState.onGround && JUMPS_LEFT > 0 && canDoubleJump)
                DoubleJump();
            jumpQueued = false;
        }

        // --- Variable jump cut ---
        // If the player releases the jump button while still rising, cut velocity
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

        // --- Dash ---
        if (dashQueued && !cState.dashing && dashCooldownTimer <= 0)
        {
            if (cState.onGround || (!cState.onGround && !airDashed))
                Dash();
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
        if (useInput && !cState.wallSliding)
            velocity.x = moveDirection * GetCurrentSpeed();

        velocity = ApplyExtraVelocities(velocity);
        rb2d.linearVelocity = velocity;

        if (moveDirection > 0.01f) FacingDirection = 1;
        else if (moveDirection < -0.01f) FacingDirection = -1;

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
        // FIX 5: Grant the midair jump budget here, on landing — not inside Jump().
        // Setting it inside Jump() gave the player a free double jump the instant they
        // left the ground, before they were even considered airborne.
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

        AudioManager.instance.PlaySFX(AudioManager.instance.fighting);
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

        AudioManager.instance.PlaySFX(AudioManager.instance.jumping);
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

        AudioManager.instance.SFXSource.PlayOneShot(AudioManager.instance.dashing, 0.2f);
    }

    private void EndDash()
    {
        cState.dashing = false;
        rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
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
        return Input.GetAxisRaw("Horizontal");
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
        return Input.GetAxisRaw("Vertical");
#endif
    }

    private bool ReadJumpInput()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump");
#endif
    }

    private bool ReadJumpInputHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);
#else
        return Input.GetKey(KeyCode.Space) || Input.GetButton("Jump");
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
        return Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftShift);
#else
        return Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftShift);
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