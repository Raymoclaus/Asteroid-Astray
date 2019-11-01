using UnityEngine;

namespace QuestSystem.Requirements
{
	public class InteractionQReq : QuestRequirement
	{
		public InteractionQReq(IInteractionWaypoint waypoint,
			string description)
			: base(description, waypoint)
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
	}
}