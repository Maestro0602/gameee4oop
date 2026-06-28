using UnityEngine;

/// <summary>
/// Attach this to your FIRST attack state (Attack1) in the Animator.
/// It enables the weapon hitbox on enter, disables on exit,
/// and triggers the next combo if the player pressed attack during the swing.
/// </summary>
public class MeleeEntryState : MeleeBaseState
{
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
            // Make sure weapon starts OFF
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
                    Debug.Log("[MeleeEntryState] Hitbox TURNED ON!");
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

        // Combo Trigger
        if (stateInfo.normalizedTime >= 0.7f && shouldCombo)
        {
            animator.SetTrigger("MeleeCombo1");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (HeroController.instance != null)
        {
            // Failsafe disable
            if (HeroController.instance.CurrentMeleeWeapon != null)
                HeroController.instance.CurrentMeleeWeapon.DisableWeapon();

            if (!shouldCombo)
                HeroController.instance.EndAttack();
        }
    }
}
