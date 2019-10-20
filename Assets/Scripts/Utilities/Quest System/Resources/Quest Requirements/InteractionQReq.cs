using UnityEngine;

namespace QuestSystem.Requirements
{
	public class InteractionQReq : QuestRequirement
	{
		private Vector3? location;
		private IInteractionWaypoint waypoint;

		public InteractionQReq(IInteractionWaypoint waypoint,
			string description)
			: base(description)
		{
			this.waypoint = waypoint;
		}

		public override void Activate()
		{
			base.Activate();
			waypoint.OnWaypointReached += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			waypoint.OnWaypointReached -= EvaluateEvent;
		}

		private void EvaluateEvent()
		{
			QuestRequirementCompleted();
		}

		public override string GetDescription => description;

		public override IWaypoint GetWaypoint
			=> waypoint;
	}

}