using System;
using System.Collections.Generic;
using UnityEngine;

namespace TriggerSystem.Triggers
{
	using IReceiver = ITriggerMessageReceiver;

	public class VicinityTrigger : MonoBehaviour, ITrigger
	{
		private HashSet<IReceiver> receivers = new HashSet<IReceiver>();
		private Collider2D col;
		private Collider2D Col => col ?? (col = GetComponent<Collider2D>());
		[SerializeField] private Transform pivot;
		private int layer = -1;
		public int TriggerLayer => layer != -1 ? layer : gameObject.layer;

		protected HashSet<IActor> nearbyActors = new HashSet<IActor>();

		public event Action<IActor> OnEnteredTrigger, OnExitedTrigger;
		public event Action OnAllExitedTrigger;

		protected virtual void Awake() => GetReceivers();

		private void OnTriggerEnter2D(Collider2D collision)
		{
			AddActor(collision.GetComponentInParent<IActor>());
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			RemoveActor(collision.GetComponentInParent<IActor>());
		}

		protected virtual void GetReceivers()
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

		protected virtual void AddActor(IActor actor)
		{
			if (!ShouldAddActor(actor)) return;

			nearbyActors.Add(actor);
			actor.OnDisabled += RemoveActor;
			EnteredTrigger(actor);
			actor.EnteredTrigger(this);
			OnEnteredTrigger?.Invoke(actor);
		}

		public virtual bool ShouldAddActor(IActor actor)
			=> actor != null && !nearbyActors.Contains(actor);

		protected virtual void RemoveActor(IActor actor)
		{
			if (actor != null && nearbyActors.Contains(actor))
			{
				nearbyActors.Remove(actor);
				actor.OnDisabled -= RemoveActor;
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
		{
			foreach (IReceiver receiver in receivers)
			{
				receiver.EnteredTrigger(actor);
			}
		}

		protected virtual void ExitedTrigger(IActor actor)
		{
			foreach (IReceiver receiver in receivers)
			{
				receiver.ExitedTrigger(actor);
			}
		}

		protected virtual void AllExitedTrigger()
		{
			foreach (IReceiver receiver in receivers)
			{
				receiver.AllExitedTrigger();
			}
		}

		public void SetCollider(Collider2D col) => this.col = col;

		public void SetPivot(Transform t) => pivot = t;

		private int NearbyActorCount => nearbyActors.Count;

		public bool TriggerEnabled
		{
			get => col.enabled;
			set => col.enabled = value;
		}

		public Vector3 PivotPosition => pivot != null ? pivot.position
			: transform.position;
	}
}