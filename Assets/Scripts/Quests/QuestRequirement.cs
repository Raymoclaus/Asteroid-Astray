using UnityEngine;

public abstract class QuestRequirement
{
	private const string NO_DESCRIPTION = "This goal has no description.";
	public bool Completed { get; private set; }
	protected bool active = false;

	public delegate void QuestRequirementUpdatedEventHandler();
	public event QuestRequirementUpdatedEventHandler OnQuestRequirementUpdated;
	public void QuestRequirementUpdated() => OnQuestRequirementUpdated?.Invoke();

	public delegate void QuestRequirementCompletedEventHandler();
	public event QuestRequirementCompletedEventHandler OnQuestRequirementCompleted;
	public void QuestRequirementCompleted()
	{
		Completed = true;
		OnQuestRequirementCompleted?.Invoke();
	}

	public virtual string GetDescription() => NO_DESCRIPTION;

	public virtual Vector3? TargetLocation() => null;

	public virtual void Activate() => active = true;
}
