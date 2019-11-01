using InputHandlerSystem;

namespace TriggerSystem.Actors.Interactors
{
	public class InputBasedInteractor : BasicInteractor
	{
		public override bool IsPerformingAction(string action)
			=> InputManager.GetInputDown(action);
	}
}