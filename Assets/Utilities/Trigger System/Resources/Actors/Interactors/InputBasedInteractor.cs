using InputHandler;

namespace TriggerSystem.Actors.Interactors
{
	public class InputBasedInteractor : BasicInteractor
	{
		public override bool IsPerformingAction(string action)
			=> InputManager.GetInput(action) >= 0f;
	}

}