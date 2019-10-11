public class DestructibleInteractableObject : InteractableObject
{
	protected override void PerformAction(IInteractor interactor)
	{
		Destroy(gameObject);
	}
}