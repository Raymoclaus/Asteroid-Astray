using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TriggerSystem
{
	using System;
	using InputHandlerSystem;
	using QuestSystem;
	using TriggerSystem.Triggers;

	public class InteractionWaypoint : MonoBehaviour, IInteractionWaypoint
	{
		public event Action OnWaypointReached;
		public IInteractor expectedInteractor;
		private IActionTrigger trigger;
		protected bool subscribed;
		private Vector3 triggerPos;

		private void Awake()
		{
			if (TriggerIsNull) return;
			Subscribe();
		}

		private void LateUpdate()
		{
			if (TriggerIsNull) return;
			triggerPos = trigger.PivotPosition;
		}

		public void SetTrigger(IActionTrigger trigger)
		{
			this.trigger = trigger;
			Subscribe();
		}

		private void Subscribe()
		{
			if (TriggerIsNull || subscribed) return;
			trigger.OnInteracted += Interacted;
			subscribed = true;
		}

		private void Unsubscribe()
		{
			if (TriggerIsNull || !subscribed) return;
			trigger.OnInteracted -= Interacted;
			subscribed = false;
		}

		private void OnDestroy()
		{
			Unsubscribe();
		}

		private void Interacted(IInteractor interactor)
		{
			if (interactor != expectedInteractor) return;
			OnWaypointReached?.Invoke();
		}

		public Vector3 WaypointPosition => !TriggerIsNull
			? trigger.PivotPosition : triggerPos;

		protected bool TriggerIsNull
			=> trigger == null
			|| trigger.Equals(null);

		public static InteractionWaypoint CreateWaypoint(IInteractor expectedInteractor,
			Vector3 position, float radius, InputAction action)
		{
			InteractionTrigger trigger = new GameObject("New Trigger").AddComponent<InteractionTrigger>();
			CircleCollider2D circleCol = trigger.gameObject.AddComponent<CircleCollider2D>();
			circleCol.radius = radius;

			trigger.InteractAction = action;
			trigger.SetCollider(circleCol);
			trigger.SetPivot(trigger.transform);
			trigger.transform.position = position;

			return CreateWaypoint(expectedInteractor, trigger, position);
		}

		public static InteractionWaypoint CreateWaypoint(
			IInteractor expectedInteractor, IActionTrigger trigger, Vector3 position)
		{
			InteractionWaypoint waypointHolder
				= new GameObject("New Waypoint").AddComponent<InteractionWaypoint>();
			waypointHolder.transform.position = position;
			waypointHolder.expectedInteractor = expectedInteractor;
			waypointHolder.SetTrigger(trigger);
			return waypointHolder;
		}
	}
}