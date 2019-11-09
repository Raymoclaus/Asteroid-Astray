using System;
using System.Collections.Generic;
using UnityEngine;

namespace TriggerSystem.Triggers
{
	using IReceiver = IActionMessageReceiver;

	public class InteractionTrigger : VicinityTrigger, IActionTrigger
	{
		[SerializeField] protected bool enabledInteractionActions = true;
		[SerializeField] protected string action = "Interact";
		private HashSet<IReceiver> receivers = new HashSet<IReceiver>();
		private HashSet<IInteractor> nearbyInteractors = new HashSet<IInteractor>();

		public event Action<IInteractor> OnInteracted;

		protected override void Awake()
		{
			base.Awake();
			GetReceivers();
		}

		private void Update()
		{
			RemoveNullInteractors();

			foreach (IInteractor interactor in nearbyInteractors)
			{
				if (interactor.StartedPerformingAction(ActionRequired))
				{
					Interact(interactor);
				}
			}
		}

		private void RemoveNullInteractors()
		{
			nearbyInteractors.RemoveWhere(t => t == null);
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
			foreach (IReceiver receiver in receivers)
			{
				receiver.Interacted(interactor, ActionRequired);
			}
			OnInteracted?.Invoke(interactor);
		}
	}
}