using InputHandlerSystem;

namespace TriggerSystem
{
	public interface IInteractor : IActor
	{
		bool StartedPerformingAction(GameAction action);
		object ObjectOrderRequest(object order);
		void Interact(object interactableObject);
	}
}
