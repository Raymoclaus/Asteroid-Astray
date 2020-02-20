using InputHandlerSystem;
using UnityEngine;

namespace MovementBehaviours
{
	public class InputMovementBehaviour : MovementBehaviour
	{
		[SerializeField] private GameAction upAction, rightAction, downAction, leftAction;

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
				InputManager.GetInput(rightAction) - InputManager.GetInput(leftAction),
				InputManager.GetInput(upAction) - InputManager.GetInput(downAction));
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