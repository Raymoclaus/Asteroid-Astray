public interface IInteractor : IActor
{
	bool IsPerformingAction(string action);
	object ObjectOrderRequest(object order);
	void Interact(object interactableObject);
}
