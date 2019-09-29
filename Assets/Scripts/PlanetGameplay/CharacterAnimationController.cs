using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
	[SerializeField] private Animator anim;

	[Header("Animator Property Names")]
	[SerializeField] private string movingName = "Moving";
	[SerializeField] private string directionName = "Direction";
	[SerializeField] private string deathName = "Death";
	[SerializeField] private string rollName = "Roll";
	[SerializeField] private string blockName = "Block";

	private bool CanAnimate
		=> anim != null
		&& anim.runtimeAnimatorController != null;

	public void SetDirection(int direction)
	{
		if (!CanAnimate) return;
		anim.SetInteger(directionName, direction);
	}

	public void SetRunning(bool running)
	{
		if (!CanAnimate) return;
		anim.SetBool(movingName, running);
	}

	public void SetRolling()
	{
		if (!CanAnimate) return;
		anim.SetTrigger(rollName);
	}

	public void SetBlocking(bool blocking)
	{
		if (!CanAnimate) return;
		anim.SetBool(blockName, blocking);
	}

	public void Die()
	{
		if (!CanAnimate) return;
		anim.SetTrigger(deathName);
	}
}
