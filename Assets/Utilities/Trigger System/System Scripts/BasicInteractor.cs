using UnityEngine;

namespace TriggerSystem.Actors.Interactors
{
	public class BasicInteractor : MonoBehaviour, IInteractor
	{
		[SerializeField] private bool canTriggerPrompts;

		public bool CanTriggerPrompts => canTriggerPrompts;

		public virtual void EnteredTrigger(ITrigger vTrigger) { }

		public virtual void ExitedTrigger(ITrigger vTrigger) { }

		public virtual void Interact(object interactableObject) { }

		public virtual bool IsPerformingAction(string action) => false;

		public virtual object ObjectOrderRequest(object order) => null;
	}
}