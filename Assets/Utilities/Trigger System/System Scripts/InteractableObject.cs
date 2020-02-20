using InputHandlerSystem;
using UnityEngine;

namespace TriggerSystem.MessageReceivers
{
	public abstract class InteractableObject : MonoBehaviour, IActionMessageReceiver
	{
		[SerializeField] private GameAction interactionAction;

		public void Interacted(IInteractor interactor, GameAction action)
		{
			if (VerifyAction(action))
			{
				PerformAction(interactor);
			}
		}

		protected abstract void PerformAction(IInteractor interactor);

		private bool VerifyAction(GameAction action) => action == interactionAction;
	}
}
