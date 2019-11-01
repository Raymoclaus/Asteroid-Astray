using System;
using TriggerSystem.Triggers;
using UnityEngine;

namespace TriggerSystem
{
	using QuestSystem;

	public class Waypoint : MonoBehaviour, IWaypoint
	{
		public event Action OnWaypointReached;
		public IActor expectedActor;
		private ITrigger trigger;
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

		public void SetTrigger(ITrigger trigger)
		{
			this.trigger = trigger;
			Subscribe();
		}

		private void Subscribe()
		{
			if (TriggerIsNull || subscribed) return;
			trigger.OnEnteredTrigger += EnteredTrigger;
			subscribed = true;
		}

		private void Unsubscribe()
		{
			if (TriggerIsNull || !subscribed) return;
			trigger.OnEnteredTrigger -= EnteredTrigger;
			subscribed = false;
		}

		private void OnDestroy()
		{
			Unsubscribe();
		}

		private void EnteredTrigger(IActor actor)
		{
			if (actor != expectedActor) return;
			OnWaypointReached?.Invoke();
		}

		public Vector3 WaypointPosition => !TriggerIsNull
			? trigger.PivotPosition : triggerPos;

		protected bool TriggerIsNull
			=> trigger == null
			|| trigger.Equals(null);

		public static Waypoint CreateWaypoint(IActor expectedActor, Vector3 position, float radius)
		{
			VicinityTrigger trigger = new GameObject("New Trigger").AddComponent<VicinityTrigger>();
			CircleCollider2D circleCol = trigger.gameObject.AddComponent<CircleCollider2D>();
			circleCol.radius = radius;

			trigger.SetCollider(circleCol);
			trigger.SetPivot(trigger.transform);
			trigger.transform.position = position;

			return CreateWaypoint(expectedActor, trigger, position);
		}

		public static Waypoint CreateWaypoint(IActor expectedActor, ITrigger trigger, Vector3 position)
		{
			Waypoint waypointHolder = new GameObject("New Waypoint").AddComponent<Waypoint>();
			waypointHolder.transform.position = position;
			waypointHolder.expectedActor = expectedActor;
			waypointHolder.SetTrigger(trigger);
			return waypointHolder;
		}
	}
}