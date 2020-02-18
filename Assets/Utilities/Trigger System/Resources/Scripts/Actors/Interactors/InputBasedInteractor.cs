using InputHandlerSystem;

namespace TriggerSystem.Actors.Interactors
{
	public class InputBasedInteractor : BasicInteractor
	{
		public override bool StartedPerformingAction(InputAction action)
			=> InputManager.GetInputDown(action);
	}
}