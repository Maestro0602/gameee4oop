using UnityEngine;

/// <summary>
/// Base class for melee combo states. Attach to Animator states.
/// Buffers attack input so the player can queue the next hit
/// during the current swing animation.
/// </summary>
public class MeleeBaseState : StateMachineBehaviour
{
    protected bool shouldCombo;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        shouldCombo = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Buffer attack input during the animation
        if (HeroController.instance != null && HeroController.instance.ConsumeAttackInput())
        {
            shouldCombo = true;
        }
    }
}
