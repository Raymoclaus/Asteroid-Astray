using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TriggerSystem.Triggers
{
	using IReceiver = IActionMessageReceiver;

	public class InteractionTrigger : VicinityTrigger, IActionTrigger
	{
		[SerializeField] protected bool enabledInteractionActions = true;
		[SerializeField] protected string action = "Interact";
		private List<IReceiver> receivers = new List<IReceiver>();
		private List<IInteractor> nearbyInteractors = new List<IInteractor>();

		public event Action<IInteractor> OnInteracted;

		private void Update()
		{
			RemoveNullInteractors();

			for (int i = 0; i < nearbyInteractors.Count; i++)
			{
				IInteractor interactor = nearbyInteractors[i];
				if (interactor.IsPerformingAction(ActionRequired))
				{
					OnInteracted?.Invoke(interactor);
				}
			}
		}

		private void RemoveNullInteractors()
		{
			nearbyInteractors.RemoveAll(t => t == null);
		}

		protected override void AddActor(IActor actor)
		{
			base.AddActor(actor);
			IInteractor interactor = actor as IInteractor;
			if (interactor == null) return;
			nearbyInteractors.Add(interactor);
		}

		protected override void RemoveActor(IActor actor)
		{
			base.RemoveActor(actor);
			IInteractor interactor = actor as IInteractor;
			if (interactor == null || !nearbyInteractors.Contains(interactor)) return;
			nearbyInteractors.Remove(interactor);
		}

		private void GetReceivers()
		{
			Transform t = transform;
			while (t != null)
			{
				foreach (IReceiver receiver in t.GetComponents<IReceiver>())
				{
					receivers.Add(receiver);
				}
				t = t.parent;
			}
		}

		public virtual bool CanBeInteractedWith
		{
			get => enabledInteractionActions;
			set => enabledInteractionActions = value;
		}

		public string ActionRequired
		{
			get => action;
			set => action = value;
		}

		public virtual void Interact(IInteractor interactor)
		{
			receivers.ForEach(t => t.Interacted(interactor, ActionRequired));
			OnInteracted?.Invoke(interactor);
		}
	}

}