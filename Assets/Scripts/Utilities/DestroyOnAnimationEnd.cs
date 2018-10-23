using UnityEngine;

//this goes onto an animator state
public class DestroyOnAnimationEnd : StateMachineBehaviour
{
	public bool destroyParent = false;
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.enabled = false;
		if (destroyParent)
		{
			Destroy(animator.transform.parent.gameObject);
		}
		else
		{
			Destroy(animator.gameObject);
		}
	}
}
