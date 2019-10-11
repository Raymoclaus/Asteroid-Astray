using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IActionMessageReceiver
{
	[SerializeField] private string interactionAction;

	public void Interacted(IInteractor interactor, string action)
	{
		if (VerifyAction(action))
		{
			PerformAction(interactor);
		}
	}

	protected abstract void PerformAction(IInteractor interactor);

	private bool VerifyAction(string action) => action == interactionAction;
}
