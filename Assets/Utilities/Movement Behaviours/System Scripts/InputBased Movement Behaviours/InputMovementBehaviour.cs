using InputHandlerSystem;
using UnityEngine;

namespace MovementBehaviours
{
	public class InputMovementBehaviour : MovementBehaviour
	{
		private void Awake()
		{
			InputManager.SetContext("Ground");
		}

		public override void TriggerUpdate()
		{
			GetMovementInput();
		}

		private void GetMovementInput()
		{
			Vector2 direction = new Vector2(
				InputManager.GetInput("Right") - InputManager.GetInput("Left"),
				InputManager.GetInput("Up") - InputManager.GetInput("Down"));
			if (direction != Vector2.zero)
			{
				MoveInDirection(direction);
			}
			else
			{
				SlowDown();
			}
		}

		public void Activate(bool activate)
		{
			enabled = activate;
			if (!activate)
			{
				SlowDown();
			}
		}
	}

}