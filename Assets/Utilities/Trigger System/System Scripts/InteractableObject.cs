using InputHandlerSystem;
using UnityEngine;

namespace TriggerSystem.MessageReceivers
{
	public abstract class InteractableObject : MonoBehaviour, IActionMessageReceiver
	{
		[SerializeField] private InputAction interactionAction;

		public void Interacted(IInteractor interactor, InputAction action)
		{
			if (VerifyAction(action))
			{
				PerformAction(interactor);
			}
		}

		protected abstract void PerformAction(IInteractor interactor);

		private bool VerifyAction(InputAction action) => action == interactionAction;
	}
}
