using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
	[SerializeField] private Animator anim;

	[Header("Animator Property Names")]
	[SerializeField] private string movingName = "Moving";
	[SerializeField] private string directionName = "Direction";

	private bool running = false;
	private int direction = 0;

	public void SetRunning(bool running)
	{
		this.running = running;
		if (anim.runtimeAnimatorController == null) return;
		anim.SetBool(movingName, running);
	}

	public void SetDirection(int direction)
	{
		this.direction = direction;
		if (anim.runtimeAnimatorController == null) return;
		anim.SetInteger(directionName, direction);
	}
}
