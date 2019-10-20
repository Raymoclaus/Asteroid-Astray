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
		private List<IReceiver> receivers = new List<IReceiver>();

		public event Action<IInteractor> OnInteracted;

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