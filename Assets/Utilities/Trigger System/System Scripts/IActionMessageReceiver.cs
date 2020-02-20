using InputHandlerSystem;

namespace TriggerSystem
{
	public interface IActionMessageReceiver
	{
		void Interacted(IInteractor interactor, GameAction action);
	}

}