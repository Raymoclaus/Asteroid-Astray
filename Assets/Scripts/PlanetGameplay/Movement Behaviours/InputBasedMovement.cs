using UnityEngine;
using InputHandler;

public class InputBasedMovement : MovementBehaviour
{
	public bool takesInput = true;

	protected void Update()
	{
		GetMovementInput();
	}

	private void GetMovementInput()
	{
		if (!takesInput)
		{
			PhysicsController.SlowDown();
			return;
		}

		Vector2 direction = Vector2.zero;
		if (InputManager.GetInput("Up"))
		{
			direction.y += 1;
		}
		if (InputManager.GetInput("Left"))
		{
			direction.x -= 1;
		}
		if (InputManager.GetInput("Down"))
		{
			direction.y -= 1;
		}
		if (InputManager.GetInput("Right"))
		{
			direction.x += 1;
		}
		if (direction != Vector2.zero)
		{
			PhysicsController.MoveInDirection(direction);
		}
		else
		{
			PhysicsController.SlowDown();
		}
	}
}
