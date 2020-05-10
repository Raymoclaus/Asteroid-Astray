using SaveSystem;

namespace QuestSystem
{
	public abstract class QuestRequirement
	{
		protected string description;
		protected IWaypoint waypoint;
		protected string WaypointID { get; set; } = string.Empty;
		public int id;

		public QuestRequirement(string description, IWaypoint waypoint)
		{
			this.description = description;
			this.waypoint = waypoint;
			WaypointID = waypoint?.UniqueID ?? string.Empty;
		}

		public bool Completed { get; private set; }
		protected bool active = false;

		public delegate void QuestRequirementUpdatedEventHandler();
		public event QuestRequirementUpdatedEventHandler OnQuestRequirementUpdated;
		public void QuestRequirementUpdated()
		{
			SteamPunkConsole.WriteLine($"Requirement Updated: {GetDescription}");
			OnQuestRequirementUpdated?.Invoke();
		}

		public delegate void QuestRequirementCompletedEventHandler();
		public event QuestRequirementCompletedEventHandler OnQuestRequirementCompleted;
		public virtual void QuestRequirementCompleted()
		{
			Completed = true;
			OnQuestRequirementCompleted?.Invoke();
		}

		public virtual string GetDescription => description;

		public virtual string GetWaypointID => WaypointID;

		public virtual void Activate() => active = true;

		public string SaveTagName => $"Quest Requirement:{id}";

		private const string DESCRIPTION_VAR_NAME = "Description",
			WAYPOINTID_VAR_NAME = "Waypoint ID",
			COMPLETED_VAR_NAME = "Completed",
			ACTIVE_VAR_NAME = "Active",
			REQUIREMENT_TYPE_VAR_NAME = "Requirement Type";

		public abstract string GetRequirementType();

		public virtual void Save(string filename, SaveTag parentTag)
		{
			//create main tag
			SaveTag mainTag = new SaveTag(SaveTagName, parentTag);
			//save description
			DataModule module = new DataModule(DESCRIPTION_VAR_NAME, description);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save requirement type
			module = new DataModule(REQUIREMENT_TYPE_VAR_NAME, GetRequirementType());
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save waypoint ID
			module = new DataModule(WAYPOINTID_VAR_NAME, WaypointID);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save completed state
			module = new DataModule(COMPLETED_VAR_NAME, Completed);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
			//save active state
			module = new DataModule(ACTIVE_VAR_NAME, active);
			UnifiedSaveLoad.UpdateOpenedFile(filename, mainTag, module);
		}
	}
}