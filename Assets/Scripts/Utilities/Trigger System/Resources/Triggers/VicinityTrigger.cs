using System;
using System.Collections.Generic;
using UnityEngine;

namespace TriggerSystem.Triggers
{
	using IReceiver = ITriggerMessageReceiver;

	public class VicinityTrigger : MonoBehaviour, ITrigger
	{
		private List<IReceiver> receivers = new List<IReceiver>();
		private Collider2D col;
		private Collider2D Col => col ?? (col = GetComponent<Collider2D>());
		[SerializeField] private Transform pivot;
		private int layer = -1;
		public int TriggerLayer => layer != -1 ? layer : gameObject.layer;

		protected List<IActor> nearbyActors = new List<IActor>();

		public event Action<IActor> OnEnteredTrigger, OnExitedTrigger;
		public event Action OnAllExitedTrigger;

		private void Awake() => GetReceivers();

		private void OnTriggerEnter2D(Collider2D collision)
		{
			AddActor(collision.GetComponentInParent<IActor>());
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			RemoveActor(collision.GetComponentInParent<IActor>());
		}

		private void GetReceivers()
		{
			Transform t = transform;
			while (t != null)
			{
				foreach (IReceiver receiver in t.GetComponents<IReceiver>())
				{
					if (receiver.CanReceiveMessagesFromLayer(TriggerLayer))
					{
						receivers.Add(receiver);
					}
				}
				t = t.parent;
			}
		}

		private void AddActor(IActor actor)
		{
			if (ShouldAddActor(actor))
			{
				nearbyActors.Add(actor);
				EnteredTrigger(actor);
				actor.EnteredTrigger(this);
				OnEnteredTrigger?.Invoke(actor);
			}
		}

		public virtual bool ShouldAddActor(IActor actor)
			=> actor != null && !nearbyActors.Contains(actor);

		private void RemoveActor(IActor actor)
		{
			if (actor != null && nearbyActors.Contains(actor))
			{
				nearbyActors.Remove(actor);
				ExitedTrigger(actor);
				actor.ExitedTrigger(this);
				OnExitedTrigger?.Invoke(actor);
			}

			if (NearbyActorCount == 0)
			{
				AllExitedTrigger();
				OnAllExitedTrigger?.Invoke();
			}
		}

		protected virtual void EnteredTrigger(IActor actor)
			=> receivers.ForEach(t => t.EnteredTrigger(actor));

		protected virtual void ExitedTrigger(IActor actor)
			=> receivers.ForEach(t => t.ExitedTrigger(actor));

		protected virtual void AllExitedTrigger()
			=> receivers.ForEach(t => t.AllExitedTrigger());

		private int NearbyActorCount => nearbyActors.Count;

		public bool TriggerEnabled
		{
			get => col.enabled;
			set => col.enabled = value;
		}

		public Vector3 PivotPosition => pivot != null ? pivot.position : transform.position;
	}

}