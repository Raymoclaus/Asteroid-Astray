using InputHandlerSystem;

namespace TriggerSystem
{
	public interface IInteractor : IActor
	{
		bool StartedPerformingAction(InputAction action);
		object ObjectOrderRequest(object order);
		void Interact(object interactableObject);
	}
}
