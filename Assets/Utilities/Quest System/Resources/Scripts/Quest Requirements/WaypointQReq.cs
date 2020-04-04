using SaveSystem;

namespace QuestSystem.Requirements
{
	public class WaypointQReq : QuestRequirement
	{
		public WaypointQReq(IWaypoint waypoint, string description)
			: base(description, waypoint)
		{

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

		private const string SAVE_TAG_NAME = "Waypoint Requirement";
		public override void Save(SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SAVE_TAG_NAME, parentTag);
			//save waypoint ID
			UnifiedSaveLoad.UpdateUnifiedSaveFile(mainTag, WaypointID);
		}
	}
}