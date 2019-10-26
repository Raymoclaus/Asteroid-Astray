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
		private bool subscribed;

		private void Awake()
		{
			if (trigger == null) return;
			Subscribe();
		}

		public void SetTrigger(ITrigger trigger)
		{
			this.trigger = trigger;
			Subscribe();
		}

		private void Subscribe()
		{
			if (subscribed) return;
			trigger.OnEnteredTrigger += EnteredTrigger;
			subscribed = true;
		}

		private void Unsubscribe()
		{
			if (trigger == null || !subscribed) return;
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

		public Vector3 WaypointPosition => trigger.PivotPosition;

		public ref Action GetOnWaypointReachedEvent => ref OnWaypointReached;

		public static Waypoint CreateWaypoint(IActor expectedActor, Vector3 position, float radius)
		{
			VicinityTrigger trigger = new GameObject("New Trigger").AddComponent<VicinityTrigger>();
			CircleCollider2D circleCol = trigger.gameObject.AddComponent<CircleCollider2D>();
			circleCol.radius = radius;

			trigger.SetCollider(circleCol);
			trigger.SetPivot(trigger.transform);
			trigger.transform.position = position;

			return CreateWaypoint(expectedActor, trigger);
		}

		public static Waypoint CreateWaypoint(IActor expectedActor, ITrigger trigger)
		{
			Waypoint waypointHolder = new GameObject("New Waypoint").AddComponent<Waypoint>();
			waypointHolder.expectedActor = expectedActor;
			waypointHolder.SetTrigger(trigger);
			return waypointHolder;
		}
	}
}