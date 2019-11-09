namespace TriggerSystem
{
	public interface IInteractor : IActor
	{
		bool StartedPerformingAction(string action);
		object ObjectOrderRequest(object order);
		void Interact(object interactableObject);
	}
}
