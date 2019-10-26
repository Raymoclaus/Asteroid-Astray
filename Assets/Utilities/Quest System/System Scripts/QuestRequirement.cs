namespace QuestSystem
{
	public abstract class QuestRequirement
	{
		protected string description;

		public QuestRequirement(string description)
		{
			this.description = description;
		}

		public bool Completed { get; private set; }
		protected bool active = false;

		public delegate void QuestRequirementUpdatedEventHandler();
		public event QuestRequirementUpdatedEventHandler OnQuestRequirementUpdated;
		public void QuestRequirementUpdated() => OnQuestRequirementUpdated?.Invoke();

		public delegate void QuestRequirementCompletedEventHandler();
		public event QuestRequirementCompletedEventHandler OnQuestRequirementCompleted;
		public virtual void QuestRequirementCompleted()
		{
			Completed = true;
			OnQuestRequirementCompleted?.Invoke();
		}

		public virtual string GetDescription => description;

		public virtual IWaypoint GetWaypoint => null;

		public virtual void Activate() => active = true;
	}
}