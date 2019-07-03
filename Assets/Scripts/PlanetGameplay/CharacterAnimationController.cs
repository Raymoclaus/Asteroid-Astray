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
		anim.SetBool(movingName, running);
	}

	public void SetDirection(int direction)
	{
		this.direction = direction;
		anim.SetInteger(directionName, direction);
	}
}
