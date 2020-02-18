using InputHandlerSystem;
using UnityEngine;

namespace MovementBehaviours
{
	public class InputBlockBehaviour : MovementBehaviour
	{
		[SerializeField] private InputAction blockAction;

		private void Update()
		{
			if (ShouldBlock)
			{
				TriggerBlock(MovementDirection);
				IsBlocking = true;
			}

			if (ShouldStopBlocking)
			{
				TriggerStopBlocking();
				IsBlocking = false;
			}
		}

		private bool IsBlocking { get; set; }

		private bool PressingBlockInput => InputManager.GetInput(blockAction) > 0f;

		private bool ShouldBlock
			=> !IsBlocking
			&& PressingBlockInput;

		private bool ShouldStopBlocking
			=> IsBlocking
			&& !PressingBlockInput;
	}

}