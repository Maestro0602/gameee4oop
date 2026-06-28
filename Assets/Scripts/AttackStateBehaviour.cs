using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (HeroController.instance != null)
        {
            HeroController.instance.cState.attacking = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (HeroController.instance != null)
        {
            HeroController.instance.EndAttack();
        }
    }
}
