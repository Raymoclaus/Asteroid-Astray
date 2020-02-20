using InputHandlerSystem;

namespace TriggerSystem.Actors.Interactors
{
	public class InputBasedInteractor : BasicInteractor
	{
		public override bool StartedPerformingAction(GameAction action)
			=> InputManager.GetInputDown(action);
	}
}