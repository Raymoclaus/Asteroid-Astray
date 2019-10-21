using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TriggerSystem
{
	using System;
	using QuestSystem;
	using TriggerSystem.Triggers;

	public class InteractionWaypoint : MonoBehaviour, IInteractionWaypoint
	{
		public event Action OnWaypointReached;
		public IInteractor expectedInteractor;
		public IActionTrigger trigger;

		private void Awake()
		{
			trigger.OnInteracted += Interacted;
		}

		private void OnDestroy()
		{
			trigger.OnInteracted -= Interacted;
		}

		private void Interacted(IInteractor interactor)
		{
			if (interactor != expectedInteractor) return;
			OnWaypointReached?.Invoke();
		}

		public Vector3 WaypointPosition => trigger.PivotPosition;

		public static InteractionWaypoint CreateWaypoint(IInteractor expectedInteractor,
			Vector3 position, float radius, string action)
		{
			InteractionTrigger trigger = new GameObject("New Trigger").AddComponent<InteractionTrigger>();
			CircleCollider2D circleCol = trigger.gameObject.AddComponent<CircleCollider2D>();
			circleCol.radius = radius;

			trigger.ActionRequired = action;
			trigger.SetCollider(circleCol);
			trigger.SetPivot(trigger.transform);
			trigger.transform.position = position;

			return CreateWaypoint(expectedInteractor, trigger);
		}

		public static InteractionWaypoint CreateWaypoint(IInteractor expectedInteractor, IActionTrigger trigger)
		{
			InteractionWaypoint waypointHolder = new GameObject("New Waypoint").AddComponent<InteractionWaypoint>();
			waypointHolder.expectedInteractor = expectedInteractor;
			waypointHolder.trigger = trigger;
			return waypointHolder;
		}
	}
}