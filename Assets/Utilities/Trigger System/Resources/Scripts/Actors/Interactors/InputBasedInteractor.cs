using InputHandlerSystem;

namespace TriggerSystem.Actors.Interactors
{
	public class InputBasedInteractor : BasicInteractor
	{
		public override bool StartedPerformingAction(string action)
			=> InputManager.GetInputDown(action);
	}
}