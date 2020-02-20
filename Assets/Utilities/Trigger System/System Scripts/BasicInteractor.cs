using InputHandlerSystem;
using System;
using UnityEngine;

namespace TriggerSystem.Actors.Interactors
{
	public class BasicInteractor : MonoBehaviour, IInteractor
	{
		[SerializeField] private bool canTriggerPrompts;

		public event Action<IActor> OnDisabled;

		private void OnDisable() => OnDisabled?.Invoke(this);

		public bool CanTriggerPrompts => canTriggerPrompts;

		public virtual void EnteredTrigger(ITrigger vTrigger) { }

		public virtual void ExitedTrigger(ITrigger vTrigger) { }

		public virtual void Interact(object interactableObject) { }

		public virtual bool StartedPerformingAction(GameAction action) => false;

		public virtual object ObjectOrderRequest(object order) => null;

		public virtual Vector3 Position => transform.position;
	}
}