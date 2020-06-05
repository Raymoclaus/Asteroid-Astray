using UnityEngine;

namespace QuestSystem.Requirements
{
	public class WaypointQReq : QuestRequirement
	{
		protected WaypointQReq() : base()
		{

		}

		public WaypointQReq(IWaypoint waypoint, string description)
			: base(description, waypoint)
		{

		}

		public override void Activate()
		{
			base.Activate();
			Waypoint.OnWaypointReached += EvaluateEvent;
		}

		public override void QuestRequirementCompleted()
		{
			base.QuestRequirementCompleted();
			Waypoint.OnWaypointReached -= EvaluateEvent;
		}

		private void EvaluateEvent()
		{
			if (Completed)
			{
				Debug.Log("Quest requirement already completed.");
				return;
			}

			QuestRequirementCompleted();
		}
	}
}