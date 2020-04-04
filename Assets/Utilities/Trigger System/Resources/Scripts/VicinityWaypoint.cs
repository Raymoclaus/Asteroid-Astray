using System;
using TriggerSystem.Triggers;
using UnityEngine;

namespace TriggerSystem
{
	using QuestSystem;
	using SaveSystem;

	public class VicinityWaypoint : MonoBehaviour, IWaypoint
	{
		[SerializeField] private VicinityTrigger trigger;

		public IActor ExpectedActor { get; set; }
		public string ExpectedActorID => ExpectedActor?.UniqueID;

		public float Radius { get; set; }
		public string UniqueID { get; set; }

		public event Action OnWaypointReached;

		private void Awake()
		{
			Subscribe();
		}

		private void OnDestroy()
		{
			Unsubscribe();
		}

		private void Subscribe()
		{
			if (TriggerIsNull) return;
			Unsubscribe();
			trigger.OnEnteredTrigger += EnteredTrigger;
		}

		private void Unsubscribe()
		{
			if (TriggerIsNull) return;
			trigger.OnEnteredTrigger -= EnteredTrigger;
		}

		private bool TriggerIsNull => trigger == null;

		private void EnteredTrigger(IActor actor)
		{
			if (actor.UniqueID != ExpectedActorID) return;
			OnWaypointReached?.Invoke();
		}

		public Vector3 Position
		{
			get => transform.position;
			set => transform.position = value;
		}

		private const string SAVE_TAG_NAME = "Vicinity Waypoint";
		public virtual void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
		}

		public void Remove()
		{
			WaypointManager.RemoveWaypoint(this);

			Destroy(gameObject);
		}
	}
}