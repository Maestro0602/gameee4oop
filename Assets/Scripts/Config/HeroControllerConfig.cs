using System;
using Team2.SharedUtils;
using UnityEngine;

[CreateAssetMenu(menuName = "Hornet/HeroController Config")]
public class HeroControllerConfig : ScriptableObject, IIncludeVariableExtensions
{

    public bool CanPlayNeedolin
    {
        get
        {
            return this.canPlayNeedolin;
        }
    }


    public bool CanBrolly
    {
        get
        {
            return this.canBrolly;
        }
    }


    public bool CanDoubleJump
    {
        get
        {
            return this.canDoubleJump;
        }
    }


    public bool CanNailCharge
    {
        get
        {
            return this.canNailCharge;
        }
    }
    public bool CanBind
    {
        get
        {
            return this.canBind;
        }
    }
    public bool CanHarpoonDash
    {
        get
        {
            return this.canHarpoonDash;
        }
    }
    public bool ForceBareInventory
    {
        get
        {
            return this.forceBareInventory;
        }
    }
    public HeroControllerConfig.DownSlashTypes DownSlashType
    {
        get
        {
            return this.downSlashType;
        }
    }
    public string DownSlashEvent
    {
        get
        {
            return this.downSlashEvent;
        }
    }
    public float DownSpikeAnticTime
    {
        get
        {
            return this.downspikeAnticTime;
        }
    }
    public float DownSpikeTime
    {
        get
        {
            return this.downspikeTime;
        }
    }
    public float DownspikeSpeed
    {
        get
        {
            return this.downspikeSpeed;
        }
    }
    public float DownspikeRecoveryTime
    {
        get
        {
            return this.downspikeRecoveryTime;
        }
    }
    public bool DownspikeBurstEffect
    {
        get
        {
            return this.downspikeBurstEffect;
        }
    }
    public bool DownspikeThrusts
    {
        get
        {
            return this.downspikeThrusts;
        }
    }
    public float DashStabSpeed
    {
        get
        {
            return this.dashStabSpeed;
        }
    }
    public float DashStabTime
    {
        get
        {
            return this.dashStabTime;
        }
    }
    public bool ForceShortDashStabBounce
    {
        get
        {
            return this.forceShortDashStabBounce;
        }
    }
    public float DashStabBounceJumpSpeed
    {
        get
        {
            return this.dashStabBounceJumpSpeed;
        }
    }
    public int DashStabSteps
    {
        get
        {
            return this.dashStabSteps;
        }
    }
    public virtual float AttackDuration
    {
        get
        {
            return this.attackDuration;
        }
    }
    public virtual float QuickAttackSpeedMult
    {
        get
        {
            return this.quickAttackSpeedMult;
        }
    }
    public virtual float AttackRecoveryTime
    {
        get
        {
            return this.attackRecoveryTime;
        }
    }
    public virtual float AttackCooldownTime
    {
        get
        {
            return this.attackCooldownTime;
        }
    }
    public virtual float QuickAttackCooldownTime
    {
        get
        {
            return this.quickAttackCooldownTime;
        }
    }
    public bool CanTurnWhileSlashing
    {
        get
        {
            return this.canTurnWhileSlashing;
        }
    }
    public bool ChargeSlashRecoils
    {
        get
        {
            return this.chargeSlashRecoils;
        }
    }
    public float ChargeSlashLungeSpeed
    {
        get
        {
            return this.chargeSlashLungeSpeed;
        }
    }
    public float ChargeSlashLungeDeceleration
    {
        get
        {
            return this.chargeSlashLungeDeceleration;
        }
    }
    public int ChargeSlashChain
    {
        get
        {
            return this.chargeSlashChain;
        }
    }
    public bool WallSlashSlowdown
    {
        get
        {
            return this.wallSlashSlowdown;
        }
    }
    private bool IsDownSlashTypeDownSpike()
    {
        return this.downSlashType == HeroControllerConfig.DownSlashTypes.DownSpike;
    }
    private bool ShowDownSlashSpeed()
    {
        return this.downspikeThrusts && this.IsDownSlashTypeDownSpike();
    }
    private bool IsDownSlashTypeSlash()
    {
        return this.downSlashType == HeroControllerConfig.DownSlashTypes.Slash;
    }
    private bool IsDownSlashTypeCustom()
    {
        return this.downSlashType == HeroControllerConfig.DownSlashTypes.Custom;
    }
    private void OnValidate()
    {
        if (this.dashStabSteps < 1)
        {
            this.dashStabSteps = 1;
        }
    }
    //public tk2dSpriteAnimationClip GetAnimationClip(string clipName)
    //{
    //    if (!this.heroAnimOverrideLib)
    //    {
    //        return null;
    //    }
    //    return this.heroAnimOverrideLib.GetClipByName(clipName);
    //}
    public void OnUpdatedVariable(string variableName)
    {
    }
    [Header("Animation")]
    [SerializeField]
    private tk2dSpriteAnimation heroAnimOverrideLib;
    [Header("Abilities")]
    [SerializeField]
    private bool canPlayNeedolin;
    [SerializeField]
    private bool canBrolly;
    [SerializeField]
    private bool canDoubleJump;
    [SerializeField]
    private bool canNailCharge;
    [SerializeField]
    private bool canBind;
    [SerializeField]
    private bool canHarpoonDash;
    [Header("UI")]
    [SerializeField]
    private bool forceBareInventory;
    [Header("\"Constants\"")]
    [SerializeField]
    private HeroControllerConfig.DownSlashTypes downSlashType;
    [SerializeField]
    [ModifiableProperty]
    [Conditional("IsDownSlashTypeCustom", true, true, true)]
    [InspectorValidation]
    private string downSlashEvent;
    [SerializeField]
    [ModifiableProperty]
    [Conditional("IsDownSlashTypeDownSpike", true, true, true)]
    private float downspikeAnticTime;
    [SerializeField]
    [ModifiableProperty]
    [Conditional("IsDownSlashTypeDownSpike", true, true, true)]
    private float downspikeTime;
    [SerializeField]
    [ModifiableProperty]
    [Conditional("ShowDownSlashSpeed", true, true, true)]
    private float downspikeSpeed;
    [SerializeField]
    private float downspikeRecoveryTime;
    [SerializeField]
    [ModifiableProperty]
    [Conditional("IsDownSlashTypeDownSpike", true, true, true)]
    private bool downspikeBurstEffect = true;
    [SerializeField]
    [ModifiableProperty]
    [Conditional("IsDownSlashTypeDownSpike", true, true, true)]
    private bool downspikeThrusts = true;
    [Space]
    [SerializeField]
    private float dashStabSpeed;
    [SerializeField]
    private float dashStabTime;
    [SerializeField]
    private bool forceShortDashStabBounce;
    [SerializeField]
    private float dashStabBounceJumpSpeed;
    [SerializeField]
    private int dashStabSteps = 1;
    [Space]
    [SerializeField]
    private float attackDuration;
    [SerializeField]
    private float quickAttackSpeedMult;
    [SerializeField]
    private float attackRecoveryTime;
    [SerializeField]
    private float attackCooldownTime;
    [SerializeField]
    private float quickAttackCooldownTime;
    [SerializeField]
    private bool canTurnWhileSlashing;
    [Space]
    [SerializeField]
    private bool chargeSlashRecoils;
    [SerializeField]
    private float chargeSlashLungeSpeed;
    [SerializeField]
    private float chargeSlashLungeDeceleration = 1f;
    [SerializeField]
    private int chargeSlashChain;
    [SerializeField]
    private bool wallSlashSlowdown;
    public enum DownSlashTypes
    {
        DownSpike,
        Slash,
        Custom
    }
}
