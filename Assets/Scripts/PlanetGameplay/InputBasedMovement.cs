using UnityEngine;

public class InputBasedMovement : PlanetEntityMovement
{
	void Update()
	{
		GetMovementInput();
	}

	private void GetMovementInput()
	{
		Vector2 direction = Vector2.zero;
		if (Input.GetKey(KeyCode.W))
		{
			direction.y += 1;
		}
		if (Input.GetKey(KeyCode.A))
		{
			direction.x -= 1;
		}
		if (Input.GetKey(KeyCode.S))
		{
			direction.y -= 1;
		}
		if (Input.GetKey(KeyCode.D))
		{
			direction.x += 1;
		}
		if (direction != Vector2.zero)
		{
			Move(direction);
		}
		else
		{
			Stop();
		}
	}
}
