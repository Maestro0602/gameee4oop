using System;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;

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
        rb2d = GetComponent<Rigidbody2D>();
        if (cState == null)
            cState = new HeroControllerStates();
    }

    private void Update()
    {
        move_input = ReadMoveInput();
        if (!Mathf.Approximately(move_input, 0f))
            Move(move_input, true);
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
    private float GetRunSpeed() => moveSpeed;

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
        if (rb2d == null) return;
        Vector2 velocity = rb2d.linearVelocity;
        velocity.y = jumpForce;
        rb2d.linearVelocity = velocity;
    }

    public void Attack()
    {
        // TODO: implement attack logic
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

    // -------------------------------------------------------
    // Inner Types
    // -------------------------------------------------------

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