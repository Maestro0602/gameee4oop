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

    [Header("Movement - Run")]
    [SerializeField] private float RUN_SPEED = 8.3f;

    [Header("Movement - Jump")]
    [SerializeField] private float JUMP_SPEED = 20f;

    [Header("Movement - Double Jump")]
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private float doubleJumpSpeed = 20f;
    
    // Call this from a ShopItem UnityEvent to unlock Double Jump!
    public void UnlockDoubleJump()
    {
        canDoubleJump = true;
        Debug.Log("[HeroController] Double Jump Unlocked!");
    }

    [Header("Combat - Spell (Soul Shooting)")]
    [SerializeField] private bool canCastSpell = false;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellSpawnPoint;
    [SerializeField] private float spellCooldown = 1.0f;
    private float spellCooldownTimer;

    public void UnlockSpell()
    {
        canCastSpell = true;
        Debug.Log("[HeroController] Spell Unlocked!");
    }

    [Header("Combat - Charge Attack (Nail Art)")]
    [SerializeField] private bool canChargeAttack = false;
    [SerializeField] private float chargeTimeThreshold = 1.0f;
    private float chargeTimer = 0f;

    public void UnlockChargeAttack()
    {
        canChargeAttack = true;
        Debug.Log("[HeroController] Charge Attack Unlocked!");
    }

    [Header("Movement - Dash")]
    [SerializeField] private float DASH_SPEED = 20f;
    [SerializeField] private float DASH_DISTANCE = 6f;
    [SerializeField] private float DASH_COOLDOWN = 0.6f;

    [Header("Movement - Wall Climb")]
    [SerializeField] private float WALL_RAY_LENGTH = 1.2f;
    [Tooltip("Max fall speed while wall-sliding (not clinging).")]
    [SerializeField] private float WALLSLIDE_SPEED = 3f;
    [Tooltip("Gravity applied during wall slide (slow descent).")]
    [SerializeField] private float WALL_SLIDE_GRAVITY = 4f;
    [Tooltip("Speed at which the player climbs UP a wall while holding climb + up.")]
    [SerializeField] private float WALL_CLIMB_SPEED = 4f;
    [Tooltip("Speed at which the player climbs DOWN a wall while holding climb + down.")]
    [SerializeField] private float WALL_CLIMB_DOWN_SPEED = 6f;

    // Smoothing / inertia removed — movement is instant

    [Header("Movement - Assist")]
    [Tooltip("Grace period after leaving ground where jump still works.")]
    [SerializeField] private float COYOTE_TIME = 0.1f;
    [Tooltip("How long a jump press is remembered before landing.")]
    [SerializeField] private float JUMP_BUFFER_TIME = 0.12f;

    [Header("Movement - Gravity")]
    [Tooltip("Gravity applied while falling or at apex with no input.")]
    [SerializeField] private float DEFAULT_GRAVITY = 4.5f;
    [Tooltip("Reduced gravity while ascending with jump held.")]
    [SerializeField] private float AIR_HANG_GRAVITY = 3.0f;
    [Tooltip("How much to multiply gravity when falling (snappier landing).")]
    [SerializeField] private float FALL_GRAVITY_MULTIPLIER = 2.5f;
    [Tooltip("Gravity multiplier at the apex of the jump for hang time.")]
    [SerializeField] private float APEX_GRAVITY_MULTIPLIER = 0.5f;
    [Tooltip("Vertical speed threshold to count as 'at apex'. Below this, apex gravity kicks in.")]
    [SerializeField] private float APEX_SPEED_THRESHOLD = 3f;
    [SerializeField] private float MAX_FALL_VELOCITY = 28f;
    [SerializeField] private float MAX_FALL_VELOCITY_DJUMP = 20f;
    [Tooltip("Gravity multiplier when jump button is released early (short hop).")]
    [SerializeField] private float LOW_JUMP_GRAVITY_MULTIPLIER = 4.0f;

    [Header("Movement - Jump Hold")]
    [Tooltip("Seconds the button must be held to count as a 'held' jump instead of a tap.")]
    [SerializeField] private float JUMP_HOLD_THRESHOLD = 0.15f;

    // Internal State
    private bool airDashed;
    private bool wasOnGround;
    private bool isDoubleJumping;     // track if last jump was a double jump (affects fall cap)
    private float jumpHoldTimer;      // how long jump button has been held this jump

    // Wall & assist state
    private float coyoteTimer;
    private float jumpBufferTimer;
    private int wallDirection;           // -1 = wall on left, 1 = wall on right, 0 = no wall
    private bool touchingWallThisFrame;
    private bool isClimbing;             // internal climb tracking

    [Header("References")]
    [Tooltip("Place this Transform at the character's hand/sword tip in the scene.")]
    [SerializeField] private Transform attackOrigin;

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
    [SerializeField] private string animCanMove = "canMove";

    private int hashGrounded;
    private int hashXVelocity;
    private int hashYVelocity;
    private int hashDashing;
    private int hashAttack;
    private int hashAttackDir;
    private int hashJump;
    private int hashDoubleJump;
    private int hashWallSlide;
    private int hashCanMove;

    [Header("Look Config")]
    [SerializeField] private float lookDelay = 0.5f;
    private float lookTimer = 0f;

    [Header("Ground Check Config")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1.2f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private Collider2D selfCollider;
    private readonly Collider2D[] groundHits = new Collider2D[4];

    [Header("Combat - Knockback")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float knockbackDuration = 0.25f;
    [Tooltip("How long the player is invincible after taking damage")]
    [SerializeField] private float invulnerabilityDuration = 1.0f;
    private float knockbackTimer = 0f;
    private int knockbackDir = 1;
    private float invulnTimer = 0f;

    private float timeSinceLastAttack;
    private float attackCooldownTimer;
    private float attackDurationTimer;   // replaces Invoke-based EndAttack

    [Header("Combat - Attack")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackKnockbackForce = 15f;
    [SerializeField] private MeleeWeapon forwardWeapon;
    [SerializeField] private MeleeWeapon upWeapon;
    [SerializeField] private MeleeWeapon downWeapon;
    
    /// <summary>Public accessor for StateMachineBehaviours to reach the weapon.</summary>
    public MeleeWeapon CurrentMeleeWeapon
    {
        get
        {
            if (cState.upAttacking) return upWeapon != null ? upWeapon : forwardWeapon;
            if (cState.downAttacking) return downWeapon != null ? downWeapon : forwardWeapon;
            return forwardWeapon;
        }
    }
    
    [Header("Combat - Recoil")]
    [SerializeField] private float pogoForce = 15f;
    [SerializeField] private float horizontalRecoilForce = 8f;

    private Rigidbody2D rb2d;
    public ActorStates hero_state;
    public ActorStates prev_hero_state;

    [SerializeField] private float move_input;
    private readonly List<DecayingVelocity> extraAirMoveVelocities = new List<DecayingVelocity>();

    // --- Properties ---
    public bool ShowRuntimeHitbox { get => showHeroHitbox; set => showHeroHitbox = value; }
    public int FacingDirection { get; private set; } = 1;
    public Vector2 AttackOriginPosition
    {
        get
        {
            // 1. If the user explicitly assigned an Attack Origin, use its exact position.
            if (attackOrigin != null) return attackOrigin.position;
            
            // 2. Fallback to the visual center of the Sprite (most accurate to the character's body)
            SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite != null) return sprite.bounds.center;

            // 3. Fallback to the physical collider center
            if (selfCollider != null) return selfCollider.bounds.center;
            
            // 4. Absolute fallback
            return transform.position;
        }
    }
    public Rigidbody2D Body => rb2d;

    public bool CanMove
    {
        get
        {
            // Hollow Knight style: always allow movement during attacks
            if (cState.attacking) return true;
            
            if (anim != null && animatorParams.Contains(hashCanMove))
            {
                return anim.GetBool(hashCanMove);
            }
            return true;
        }
    }


    // -------------------------------------------------------
    // Jump state
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

        // Auto-configure Attack components if missing
        if (attackOrigin == null)
        {
            GameObject originGo = new GameObject("AttackOrigin_Auto");
            originGo.transform.SetParent(transform);
            
            // Position at the center of the sprite (not the feet!)
            SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                // Convert sprite world center to local space of this transform
                originGo.transform.position = sprite.bounds.center;
            }
            else if (selfCollider != null)
            {
                originGo.transform.position = selfCollider.bounds.center;
            }
            else
            {
                originGo.transform.localPosition = Vector3.zero;
            }
            
            attackOrigin = originGo.transform;
        }

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
        hashCanMove = Animator.StringToHash(animCanMove);

        // Clamp insane inspector values to prevent enemies from flying off-screen
        if (attackKnockbackForce > 30f) attackKnockbackForce = 15f;
        if (knockbackForce > 30f) knockbackForce = 15f;

        // Warn if MeleeWeapon is not assigned (user must set this up manually)
        if (forwardWeapon == null)
        {
            Debug.LogWarning("[HeroController] Forward Weapon is NOT assigned! " +
                "Attacks will not deal damage. Please assign a MeleeWeapon in the Inspector.");
        }

        JUMPS_LEFT = 0;
        CacheAnimatorParameters();
    }

    private readonly HashSet<int> animatorParams = new HashSet<int>();

    private void CacheAnimatorParameters()
    {
        animatorParams.Clear();
        if (anim != null)
        {
            foreach (var param in anim.parameters)
            {
                animatorParams.Add(param.nameHash);
            }
        }
    }

    private void SetAnimBool(int hash, bool value)
    {
        if (anim != null && animatorParams.Contains(hash))
            anim.SetBool(hash, value);
    }

    private void SetAnimFloat(int hash, float value)
    {
        if (anim != null && animatorParams.Contains(hash))
            anim.SetFloat(hash, value);
    }

    private void SetAnimInt(int hash, int value)
    {
        if (anim != null && animatorParams.Contains(hash))
            anim.SetInteger(hash, value);
    }

    private void SetAnimTrigger(int hash)
    {
        if (anim != null && animatorParams.Contains(hash))
            anim.SetTrigger(hash);
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

        if (ReadJumpInput()) jumpBufferTimer = JUMP_BUFFER_TIME;
        if (ReadDashInput()) dashQueued = true;

        // Track whether the jump button is still held (for variable height)
        if (!ReadJumpInputHeld()) jumpButtonHeld = false;

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashDurationTimer > 0) dashDurationTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        if (spellCooldownTimer > 0) spellCooldownTimer -= Time.deltaTime;

        timeSinceLastAttack += Time.deltaTime;
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;
        // attackDurationTimer is now handled by MeleeBaseState / MeleeComboState OnStateExit

        if (knockbackTimer > 0f) knockbackTimer -= Time.deltaTime;
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;

        if (ReadAttackInput()) DoAttack();

        // --- Spell Logic ---
        if (ReadSpellInput() && canCastSpell && spellCooldownTimer <= 0 && CanMove)
        {
            CastSpell();
        }

        // --- Charge Attack Logic ---
        if (canChargeAttack && CanMove)
        {
            if (ReadAttackInputHeld())
            {
                chargeTimer += Time.deltaTime;
            }
            else if (chargeTimer > 0f)
            {
                if (chargeTimer >= chargeTimeThreshold)
                {
                    DoChargeAttack();
                }
                chargeTimer = 0f;
            }
        }

        // --- Potion / Inventory Logic ---
        if (ReadPotionInput() && CanMove)
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.TryConsumeHealthPotion();
            }
            else
            {
                Debug.LogWarning("[HeroController] Cannot drink potion: InventoryManager is missing from the scene!");
            }
        }

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;
        SetAnimBool(hashGrounded, cState.onGround);
        SetAnimFloat(hashXVelocity, Mathf.Abs(rb2d.linearVelocity.x));
        SetAnimFloat(hashYVelocity, rb2d.linearVelocity.y);
        SetAnimBool(hashDashing, cState.dashing);
        SetAnimBool(hashWallSlide, cState.wallSliding);
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
        // Block all jumps while wall sliding — the wall climb system
        // owns vertical movement in that state.
        bool wantsJump = jumpBufferTimer > 0f && CanMove;
        if (wantsJump && !cState.wallSliding)
        {
            if ((cState.onGround || coyoteTimer > 0f) && jumpCooldownTimer <= 0f)
            {
                Jump();
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
            }
            else if (cState.isJumping && JUMPS_LEFT > 0 && canDoubleJump)
            {
                DoubleJump();
                jumpBufferTimer = 0f;
            }
        }

        // --- Track jump hold duration ---
        if (isJumping && jumpButtonHeld && rb2d.linearVelocity.y > 0f)
            jumpHoldTimer += Time.fixedDeltaTime;

        // --- Variable jump cut (tap vs held) ---
        if (isJumping && !jumpButtonHeld && rb2d.linearVelocity.y > 0f)
        {
            // HELD — keep full velocity, GMTK style gravity will handle the sharp arc
            SetIsJumping(false);
        }

        // NOTE: isJumping is NOT cleared at apex — it persists through the
        // full jump arc until landing, so double-jump can be gated on it.

        // --- Wall climb / slide ---
        HandleWallClimb();

        // --- Dash ---
        if (dashQueued && !cState.dashing && dashCooldownTimer <= 0 && CanMove)
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
        else if (!isClimbing)
        {
            bool applyInput = (knockbackTimer <= 0f) && CanMove;
            Move(applyInput ? move_input : 0f, applyInput);
            if (!cState.wallSliding)
                ApplyCustomGravity();
        }

        // --- Clamp fall speed ---
        ClampFallVelocity();
    }

    // -------------------------------------------------------
    // Knockback / Damage
    // -------------------------------------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGroundState();
        CheckForKnockback(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGroundState();
        CheckForKnockback(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        CheckForKnockback(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        CheckForKnockback(collider);
    }

    private void CheckForKnockback(Collider2D col)
    {
        // Prevent the player from hitting themselves with their own sword/hitboxes!
        if (col.transform.IsChildOf(this.transform)) return;

        // CRITICAL FIX: Ensure the enemy is actually touching the player's BODY (selfCollider), 
        // not just touching our child sword hitboxes!
        if (selfCollider != null && !selfCollider.IsTouching(col)) return;

        // Log all contacts with triggers/colliders for troubleshooting
        bool isEnemyOrHazard = col.CompareTag("Enemy") || col.CompareTag("Hazard") || col.gameObject.layer == LayerMask.NameToLayer("Enemy");
        if (isEnemyOrHazard)
        {
            Debug.Log($"[HeroController] Contact registered with Enemy/Hazard: {col.gameObject.name}. " +
                      $"Invulnerable state: {cState.invulnerable}, invulnTimer: {invulnTimer:F3}s.");
        }

        if (cState.invulnerable || invulnTimer > 0f) return;

        if (isEnemyOrHazard)
        {
            TakeDamageAndKnockback(col.transform.position);
        }
    }

    public void TakeDamageAndKnockback(Vector2 damageSourcePos)
    {
        if (cState.dashing) EndDash();
        if (cState.wallSliding || isClimbing) EndWallSlide();
        
        knockbackTimer = knockbackDuration;
        invulnTimer = invulnerabilityDuration;
        
        int knockDir = transform.position.x < damageSourcePos.x ? -1 : 1;
        Vector2 kbVelocity = new Vector2(knockDir * knockbackForce, JUMP_SPEED * 0.5f);
        rb2d.linearVelocity = kbVelocity;

        Debug.Log($"[HeroController] PLAYER HIT! Source Position: {damageSourcePos}. " +
                  $"Applying knockback velocity: {kbVelocity} (Direction: {knockDir}). " +
                  $"knockbackTimer set to: {knockbackTimer:F3}s. invulnTimer set to: {invulnTimer:F3}s.");

        if (PlayerData.instance != null)
        {
            PlayerData.instance.TakeHealth(1, false, true);
        }

#if PLAYMAKER
        // Notify the PlayMaker FSM so the UI HP Bar updates!
        PlayMakerFSM[] fsms = GetComponents<PlayMakerFSM>();
        foreach (var fsm in fsms)
        {
            fsm.SendEvent("DamagePlayer");
        }
#endif
    }

    // -------------------------------------------------------
    // Custom Gravity  ← THE FIX
    // -------------------------------------------------------

    private void ApplyCustomGravity()
    {
        // No gravity needed while grounded
        if (cState.onGround && rb2d.linearVelocity.y <= 0f) return;

        float vy = rb2d.linearVelocity.y;
        float gravityToUse = DEFAULT_GRAVITY;

        if (vy < 0f)
        {
            // Falling -> heavy gravity for snappy landing
            gravityToUse = DEFAULT_GRAVITY * FALL_GRAVITY_MULTIPLIER;
        }
        else if (vy > 0f && !jumpButtonHeld)
        {
            // Ascending but jump released -> extra heavy gravity for short hop
            gravityToUse = DEFAULT_GRAVITY * LOW_JUMP_GRAVITY_MULTIPLIER;
        }
        else if (vy > 0f && vy <= APEX_SPEED_THRESHOLD)
        {
            // Apex -> light gravity for "hang time"
            gravityToUse = DEFAULT_GRAVITY * APEX_GRAVITY_MULTIPLIER;
        }
        else if (vy > APEX_SPEED_THRESHOLD && isJumping && jumpButtonHeld)
        {
            // Ascending normally -> default gravity
            gravityToUse = DEFAULT_GRAVITY;
        }

        // Use a base gravity value of 9.81 to be safe against project settings changes (0 gravity).
        float baseGravity = Mathf.Abs(Physics2D.gravity.y) > 0.1f ? Mathf.Abs(Physics2D.gravity.y) : 9.81f;
        
        // Apply as a downward acceleration (units/s² scaled by Physics2D gravity magnitude)
        rb2d.linearVelocity += Vector2.down * gravityToUse * baseGravity * Time.fixedDeltaTime;
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
        Vector2 currentGroundCheckSize = groundCheckSize;
        if (selfCollider != null)
        {
            Bounds bounds = selfCollider.bounds;
            checkPos = new Vector2(bounds.center.x, bounds.min.y - 0.01f);
            // Use slightly less than collider width to prevent floating on edges
            currentGroundCheckSize = new Vector2(bounds.size.x * 0.9f, 0.1f);
        }
        else
            checkPos = (Vector2)transform.position + new Vector2(0f, -0.51f);

        bool previousGround = cState.onGround;

        int hitCount = Physics2D.OverlapBoxNonAlloc(checkPos, currentGroundCheckSize, 0f, groundHits, groundLayer);

        bool hitGround = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (groundHits[i] != null && groundHits[i] != selfCollider && !groundHits[i].isTrigger)
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

        // During knockback, preserve the knockback velocity — don't override it
        if (knockbackTimer > 0f)
        {
            // Only apply extra velocities (if any) and skip normal movement control
            velocity = ApplyExtraVelocities(velocity);
            rb2d.linearVelocity = velocity;
            return;
        }

        if (useInput && !cState.wallSliding)
        {
            // Instant speed — no accel/decel ramp
            float targetSpeed = moveDirection * GetCurrentSpeed();
            velocity.x = targetSpeed;
        }
        else if (!useInput && !cState.wallSliding && knockbackTimer <= 0f)
        {
            // Lock movement velocity when input is disabled (e.g., during attack)
            velocity.x = 0f;
        }

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
        airDashed = false;
        SetIsJumping(false);
        isDoubleJumping = false;
        isClimbing = false;
        cState.isClimbing = false;
        cState.jumping = false;
        cState.wallSliding = false;
        cState.touchingWall = false;
        jumpHoldTimer = 0f;
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
        // Block attacks during dash
        if (cState.dashing) return;

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
        cState.upAttacking = (attackDir == AttackDirection.upward);
        cState.downAttacking = (attackDir == AttackDirection.downward);
        Debug.Log($"[HeroController] Player triggered attack! Direction: {attackDir}");

        SetAnimInt(hashAttackDir, (int)attackDir);
        SetAnimTrigger(hashAttack);

        TrySetCorrectFacing();

        if (cState.wallSliding && attackDir != AttackDirection.normal)
        {
            cState.wallSliding = false;
        }

        DidAttack();
    }

    private void DidAttack()
    {
        timeSinceLastAttack = 0f;
        attackCooldownTimer = 0.25f;
    }

    private void CastSpell()
    {
        spellCooldownTimer = spellCooldown;
        SetAnimTrigger(Animator.StringToHash("CastSpell"));
        TrySetCorrectFacing();
        
        // Since Aseprite clips are read-only, we use a Coroutine delay instead of an Animation Event!
        StartCoroutine(SpawnSpellRoutine(0.15f)); // <-- Change this number to sync with your animation!
    }

    private System.Collections.IEnumerator SpawnSpellRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (spellPrefab != null && spellSpawnPoint != null)
        {
            GameObject spellObj = Instantiate(spellPrefab, spellSpawnPoint.position, Quaternion.identity);
            SpellProjectile spell = spellObj.GetComponent<SpellProjectile>();
            if (spell != null) spell.Fire(FacingDirection);
        }
        else
        {
            Debug.LogWarning("[HeroController] Spell Prefab or Spawn Point is missing!");
        }
    }

    private void DoChargeAttack()
    {
        Debug.Log("[HeroController] Triggered Charge Attack!");
        SetAnimTrigger(Animator.StringToHash("ChargeAttack"));
        TrySetCorrectFacing();
        
        // Let the animator handle hitboxes just like a normal attack
    }

    public void EndAttack()
    {
        // Must disable weapon BEFORE clearing up/down attack state
        if (CurrentMeleeWeapon != null)
            CurrentMeleeWeapon.DisableWeapon();

        cState.attacking = false;
        cState.upAttacking = false;
        cState.downAttacking = false;
            
        Debug.Log("[HeroController] Player stopped attacking.");
    }

    // Called via Animation Events
    public void AttackStart()
    {
        if (CurrentMeleeWeapon != null)
        {
            CurrentMeleeWeapon.EnableWeapon();
            Debug.Log("[HeroController] AttackStart animation event fired - Hitbox Enabled.");
        }
    }

    // Called via Animation Events
    public void AttackEnd()
    {
        if (CurrentMeleeWeapon != null)
        {
            CurrentMeleeWeapon.DisableWeapon();
            Debug.Log("[HeroController] AttackEnd animation event fired - Hitbox Disabled.");
        }
    }

    // -------------------------------------------------------
    // Recoil (Hollow Knight Style Pogo)
    // -------------------------------------------------------

    public void Recoil(Vector2 recoilDirection)
    {
        if (recoilDirection.y > 0)
        {
            // Pogo - bounce upwards
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, pogoForce);
            
            // Reset jump/dash state so player can jump/dash again after a successful pogo
            JUMPS_LEFT = 1;
            cState.doubleJumping = false;
            airDashed = false;
            dashQueued = false;
            SetIsJumping(false);
            
            Debug.Log("[HeroController] Pogo Recoil Applied!");
        }
        else
        {
            // Horizontal Recoil
            rb2d.linearVelocity = new Vector2(recoilDirection.x * horizontalRecoilForce, rb2d.linearVelocity.y);
            Debug.Log("[HeroController] Horizontal Recoil Applied!");
        }
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
    private float GetCurrentSpeed() => RUN_SPEED;

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
        // Break out of wall climb/slide if jumping from ground near a wall
        if (cState.wallSliding || isClimbing) EndWallSlide();

        SetIsJumping(true);
        isDoubleJumping = false;
        jumpButtonHeld = true;
        jumpHoldTimer = 0f;
        cState.jumping = true;
        cState.onGround = false; // Instantly set to false so instant pogos work
        jumpCooldownTimer = 0.2f;

        SetAnimTrigger(hashJump);

        // Single impulse — gravity handles the arc from here
        Vector2 v = rb2d.linearVelocity;
        v.y = JUMP_SPEED;
        rb2d.linearVelocity = v;
    }

    public void DoubleJump()
    {
        if (cState.wallSliding || isClimbing) EndWallSlide();

        SetIsJumping(true);
        isDoubleJumping = true;
        jumpButtonHeld = true;
        jumpHoldTimer = 0f;
        cState.doubleJumping = true;
        JUMPS_LEFT--;

        SetAnimTrigger(hashDoubleJump);

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
    }

    private void EndDash()
    {
        cState.dashing = false;
        rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
    }

    // -------------------------------------------------------
    // Wall Detection & Wall Climb
    // -------------------------------------------------------

    private void CheckWall()
    {
        touchingWallThisFrame = false;
        wallDirection = 0;

        if (cState.onGround || cState.dashing) return;

        // 3-ray fan: top, center, bottom of the collider
        Bounds bounds = selfCollider != null
            ? selfCollider.bounds
            : new Bounds(transform.position, Vector3.one);

        float halfWidth = bounds.extents.x;
        float rayDist = 0.15f; // Small distance to prevent sticking from far away

        Vector2 top    = new Vector2(bounds.center.x, bounds.max.y - 0.05f);
        Vector2 center = (Vector2)bounds.center;
        Vector2 bottom = new Vector2(bounds.center.x, bounds.min.y + 0.05f);

        // Origin for right checks
        Vector2 rightTop = new Vector2(bounds.center.x + halfWidth, top.y);
        Vector2 rightCenter = new Vector2(bounds.center.x + halfWidth, center.y);
        Vector2 rightBottom = new Vector2(bounds.center.x + halfWidth, bottom.y);

        // Origin for left checks
        Vector2 leftTop = new Vector2(bounds.center.x - halfWidth, top.y);
        Vector2 leftCenter = new Vector2(bounds.center.x - halfWidth, center.y);
        Vector2 leftBottom = new Vector2(bounds.center.x - halfWidth, bottom.y);

        // Check right side (3 rays)
        if (WallRayHit(rightTop, Vector2.right, rayDist) ||
            WallRayHit(rightCenter, Vector2.right, rayDist) ||
            WallRayHit(rightBottom, Vector2.right, rayDist))
        {
            touchingWallThisFrame = true;
            wallDirection = 1;
            return;
        }

        // Check left side (3 rays)
        if (WallRayHit(leftTop, Vector2.left, rayDist) ||
            WallRayHit(leftCenter, Vector2.left, rayDist) ||
            WallRayHit(leftBottom, Vector2.left, rayDist))
        {
            touchingWallThisFrame = true;
            wallDirection = -1;
        }
    }

    /// <summary>Single wall raycast helper — returns true if a wall is hit.</summary>
    private bool WallRayHit(Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, groundLayer);
        return hit.collider != null && hit.collider != selfCollider && !hit.collider.isTrigger;
    }

    /// <summary>
    /// Wall climb system — Hollow Knight style.
    /// Default (no climb button): player slowly slides down the wall.
    /// Holding climb button (E) + Up:   climb up at WALL_CLIMB_SPEED.
    /// Holding climb button (E) + Down: climb down at WALL_CLIMB_DOWN_SPEED.
    /// Holding climb button (E) alone:  cling (freeze in place).
    /// Holding AWAY from wall:          detach immediately.
    /// </summary>
    private void HandleWallClimb()
    {
        if (touchingWallThisFrame && !cState.onGround)
        {
            // --- UNSTICK: if holding AWAY from the wall, detach immediately ---
            bool holdingAway = (wallDirection == 1 && move_input < -0.1f) ||
                               (wallDirection == -1 && move_input > 0.1f);
            if (holdingAway)
            {
                if (cState.wallSliding || isClimbing)
                    EndWallSlide();
                return;
            }

            bool holdingClimb = ReadClimbInputHeld();
            float vInput = ReadVerticalInput();

            if (!cState.wallSliding)
            {
                // Enter wall slide state
                cState.wallSliding = true;
                cState.touchingWall = true;
                SetState(ActorStates.wall_sliding);
                // Clear jump state so stale isJumping doesn't enable
                // double-jump or other jump logic while on the wall
                SetIsJumping(false);
                JUMPS_LEFT = 1;
                jumpBufferTimer = 0f; // eat any buffered jump
            }

            Vector2 v = rb2d.linearVelocity;

            if (holdingClimb)
            {
                // CLIMB MODE — vertical input controls direction
                isClimbing = true;
                cState.isClimbing = true;
                v.x = 0f; // no horizontal drift while on wall

                if (vInput > 0.1f)
                {
                    // Climb UP
                    v.y = WALL_CLIMB_SPEED;
                }
                else if (vInput < -0.1f)
                {
                    // Climb DOWN (faster than passive slide)
                    v.y = -WALL_CLIMB_DOWN_SPEED;
                }
                else
                {
                    // CLING — no vertical input, freeze in place
                    v.y = 0f;
                }
            }
            else
            {
                // NO CLIMB BUTTON — slowly slide down (Hollow Knight default)
                isClimbing = false;
                cState.isClimbing = false;
                v.x = 0f; // zero horizontal so holding toward wall doesn't wedge player

                // Apply wall-specific gravity (lighter than normal)
                v.y -= WALL_SLIDE_GRAVITY * Time.fixedDeltaTime;

                // Clamp to max wall slide speed
                if (v.y < -WALLSLIDE_SPEED)
                    v.y = -WALLSLIDE_SPEED;
            }

            rb2d.linearVelocity = v;
        }
        else
        {
            if (cState.wallSliding || isClimbing)
                EndWallSlide();
        }
    }

    private void EndWallSlide()
    {
        cState.wallSliding = false;
        cState.touchingWall = false;
        isClimbing = false;
        cState.isClimbing = false;
    }

    /// <summary>Syncs internal isJumping with cState.isJumping.</summary>
    private void SetIsJumping(bool value)
    {
        isJumping = value;
        cState.isJumping = value;
    }

    // -------------------------------------------------------
    // Debug Gizmos
    // -------------------------------------------------------

    private void OnDrawGizmos()
    {
        if (showGroundCheckGizmo)
        {
            Vector2 checkPos;
            if (TryGetComponent<Collider2D>(out var col))
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

        // --- Wall detection debug rays ---
        if (TryGetComponent<Collider2D>(out var wallCol))
        {
            Bounds wb = wallCol.bounds;
            float hw = wb.extents.x;
            float rd = 0.15f;
            Vector2 wTop    = new Vector2(wb.center.x, wb.max.y - 0.05f);
            Vector2 wCenter = (Vector2)wb.center;
            Vector2 wBottom = new Vector2(wb.center.x, wb.min.y + 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(new Vector2(wb.center.x + hw, wTop.y),    Vector2.right * rd);
            Gizmos.DrawRay(new Vector2(wb.center.x + hw, wCenter.y), Vector2.right * rd);
            Gizmos.DrawRay(new Vector2(wb.center.x + hw, wBottom.y), Vector2.right * rd);
            Gizmos.DrawRay(new Vector2(wb.center.x - hw, wTop.y),    Vector2.left  * rd);
            Gizmos.DrawRay(new Vector2(wb.center.x - hw, wCenter.y), Vector2.left  * rd);
            Gizmos.DrawRay(new Vector2(wb.center.x - hw, wBottom.y), Vector2.left  * rd);
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

        // Removed cyan attack arc gizmo per user request
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

    private bool ReadAttackInputHeld()
    {
#if ENABLE_INPUT_SYSTEM
        bool inputSysZ = Keyboard.current != null && Keyboard.current.zKey.isPressed;
        bool legacyZ = false;
        try { legacyZ = UnityEngine.Input.GetKey(UnityEngine.KeyCode.Z); } catch { }
        return inputSysZ || legacyZ;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButton("Fire1") || Input.GetKey(KeyCode.Z);
#else
        return Input.GetKey(KeyCode.Z) || Input.GetButton("Fire1");
#endif
    }

    private bool ReadSpellInput()
    {
#if ENABLE_INPUT_SYSTEM
        bool inputSysX = Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame;
        bool legacyX = false;
        try { legacyX = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.X); } catch { }
        return inputSysX || legacyX;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.X);
#else
        return Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Fire2");
#endif
    }

    private bool ReadPotionInput()
    {
#if ENABLE_INPUT_SYSTEM
        bool inputSysQ = Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
        bool legacyQ = false;
        try { legacyQ = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Q); } catch { }
        return inputSysQ || legacyQ;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Q);
#else
        return Input.GetKeyDown(KeyCode.Q);
#endif
    }

    private bool attackInputConsumedThisFrame = false;
    
    // Called by MeleeBaseState to buffer attacks
    public bool ConsumeAttackInput()
    {
        if (attackInputConsumedThisFrame) return false;
        
        bool pressed = ReadAttackInput();
        if (pressed)
        {
            attackInputConsumedThisFrame = true;
            return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        attackInputConsumedThisFrame = false; // Reset for next frame
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

    private bool ReadClimbInputHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(KeyCode.E);
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