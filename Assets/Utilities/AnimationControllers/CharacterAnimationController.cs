using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
	[SerializeField] private Animator anim;

	[Header("Animator Property Names")]
	[SerializeField] private string movingName = "Moving";
	[SerializeField] private string directionName = "Direction",
		deathName = "Death",
		rollName = "Roll",
		blockName = "Block",
		speedMultiplierName = "SpeedMultiplier";

	[SerializeField] private float speedReferenceValue = 1f;

	private bool CanAnimate
		=> anim != null
		&& anim.runtimeAnimatorController != null;

	public void SetDirection(float angle)
	{
		if (!CanAnimate) return;
		anim.SetFloat(directionName, angle);
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

	public void SetSpeedMultiplier(float speed)
	{
		if (!CanAnimate) return;
		anim.SetFloat(speedMultiplierName, speed / speedReferenceValue);
	}

	public void Die()
	{
		if (!CanAnimate) return;
		anim.SetTrigger(deathName);
	}
}
