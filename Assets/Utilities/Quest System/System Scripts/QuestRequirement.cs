namespace QuestSystem
{
	public abstract class QuestRequirement
	{
		protected string description;
		protected IWaypoint waypoint;

		public QuestRequirement(string description, IWaypoint waypoint)
		{
			this.description = description;
			this.waypoint = waypoint;
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

		public virtual IWaypoint GetWaypoint => waypoint;

		public virtual void Activate() => active = true;
	}
}