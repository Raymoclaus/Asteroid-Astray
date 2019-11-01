using InputHandlerSystem;

namespace MovementBehaviours
{
	public class InputBlockBehaviour : MovementBehaviour
	{
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

		private bool PressingBlockInput => InputManager.GetInput("Block") > 0f;

		private bool ShouldBlock
			=> !IsBlocking
			&& PressingBlockInput;

		private bool ShouldStopBlocking
			=> IsBlocking
			&& !PressingBlockInput;
	}

}