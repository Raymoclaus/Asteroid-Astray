using InputHandler;

public class InputRollingBehaviour : MovementBehaviour
{
	private void Update()
	{
		if (ShouldRoll)
		{
			TriggerRoll(MovementDirection);
		}
	}

	private bool ShouldRoll
		=> InputManager.GetInputDown("Roll");
}
