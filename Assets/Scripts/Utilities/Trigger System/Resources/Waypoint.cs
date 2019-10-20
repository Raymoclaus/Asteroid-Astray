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
		public ITrigger trigger;

		private void Awake()
		{
			trigger.OnEnteredTrigger += EnteredTrigger;
		}

		private void OnDestroy()
		{
			trigger.OnEnteredTrigger -= EnteredTrigger;
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
			waypointHolder.trigger = trigger;
			return waypointHolder;
		}
	}
}