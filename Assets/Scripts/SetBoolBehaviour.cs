using UnityEngine;

public class SetBoolBehaviour : StateMachineBehaviour
{
    [Header("Settings")]
    [Tooltip("The name of the boolean parameter in the Animator Controller.")]
    public string boolName;
    
    [Tooltip("The value to set the parameter to when entering this state.")]
    public bool valueOnEnter = true;
    
    [Tooltip("The value to set the parameter to when exiting this state.")]
    public bool valueOnExit = false;

    // Called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(boolName))
        {
            animator.SetBool(boolName, valueOnEnter);
        }
    }

    // Called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(boolName))
        {
            animator.SetBool(boolName, valueOnExit);
        }
    }
}
