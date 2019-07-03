using UnityEngine;

public class PlanetEntityMovement : MonoBehaviour
{
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private CharacterAnimationController cac;

	[SerializeField] protected float speed = 1f;
	private bool moving = false;
	private Vector2 direction;
	private int directionID;

	protected void Move(Vector2 direction)
	{
		direction.Normalize();
		moving = true;
		this.direction = direction;
		rb.velocity = direction * speed;
		directionID = ConvertDirectionToInt(direction);
		cac?.SetDirection(directionID);
		cac?.SetRunning(true);
	}

	private int ConvertDirectionToInt(Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.y, direction.x);
		if (angle >= Mathf.PI * 0.75f)
		{
			return 3;
		}
		else if (angle > Mathf.PI * 0.25f)
		{
			return 0;
		}
		else if (angle >= Mathf.PI * -0.25f)
		{
			return 1;
		}
		else if (angle > Mathf.PI * -0.75f)
		{
			return 2;
		}
		else
		{
			return 3;
		}
	}

	protected void Stop()
	{
		moving = false;
		cac?.SetRunning(false);
		rb.velocity = Vector2.zero;
	}

	public void SetSpeed(float speed) => this.speed = speed;
}
