using UnityEngine;

/// <summary>
/// Attach this to your 2nd, 3rd, etc. attack states (Attack2, Attack3) in the Animator.
/// Same as MeleeEntryState but lets you chain to further combo hits.
/// Set nextComboTrigger to the trigger name of the NEXT attack, or leave empty if this is the last hit.
/// </summary>
public class MeleeComboState : MeleeBaseState
{
    [Tooltip("Trigger name for the next combo hit. Leave empty if this is the last hit.")]
    public string nextComboTrigger;

    [Header("Hitbox Timing (0.0 to 1.0)")]
    [Tooltip("When the hitbox turns ON (e.g. 0.2 is 20% into the animation)")]
    [Range(0f, 1f)] public float hitboxStartTime = 0.1f;
    
    [Tooltip("When the hitbox turns OFF")]
    [Range(0f, 1f)] public float hitboxEndTime = 0.5f;

    private bool weaponEnabled;
    private bool weaponDisabled;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        weaponEnabled = false;
        weaponDisabled = false;

        if (HeroController.instance != null)
        {
            HeroController.instance.cState.attacking = true;
            if (HeroController.instance.CurrentMeleeWeapon != null)
                HeroController.instance.CurrentMeleeWeapon.DisableWeapon();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        float time = stateInfo.normalizedTime % 1f;

        // Turn weapon ON
        if (!weaponEnabled && time >= hitboxStartTime && time < hitboxEndTime)
        {
            weaponEnabled = true;
            if (HeroController.instance != null)
            {
                if (HeroController.instance.CurrentMeleeWeapon != null)
                {
                    HeroController.instance.CurrentMeleeWeapon.EnableWeapon();
                    Debug.Log("[MeleeComboState] Hitbox TURNED ON!");
                }
                else
                {
                    Debug.LogWarning("⚠️ CURRENT MELEE WEAPON IS NOT ASSIGNED IN HERO CONTROLLER! THE ATTACK WILL NOT WORK!");
                }
            }
        }

        // Turn weapon OFF
        if (weaponEnabled && !weaponDisabled && time >= hitboxEndTime)
        {
            weaponDisabled = true;
            if (HeroController.instance != null && HeroController.instance.CurrentMeleeWeapon != null)
                HeroController.instance.CurrentMeleeWeapon.DisableWeapon();
        }

        // Combo trigger
        if (stateInfo.normalizedTime >= 0.7f && shouldCombo && !string.IsNullOrEmpty(nextComboTrigger))
        {
            animator.SetTrigger(nextComboTrigger);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (HeroController.instance != null)
        {
            if (HeroController.instance.CurrentMeleeWeapon != null)
                HeroController.instance.CurrentMeleeWeapon.DisableWeapon();

            if (!shouldCombo || string.IsNullOrEmpty(nextComboTrigger))
                HeroController.instance.EndAttack();
        }
    }
}
