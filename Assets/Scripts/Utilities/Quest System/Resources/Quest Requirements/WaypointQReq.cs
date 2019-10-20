using System;

namespace QuestSystem.Requirements
{
	public class WaypointQReq : QuestRequirement
	{
		private IWaypoint waypoint;

		public WaypointQReq(IWaypoint waypoint, string description)
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
			if (Completed || !active) return;

			QuestRequirementCompleted();
		}

		public override string GetDescription => description;

		public override IWaypoint GetWaypoint => waypoint;
	}
}