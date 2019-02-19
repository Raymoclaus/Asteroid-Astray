using UnityEngine;

public abstract class QuestRequirement
{
	private const string NO_DESCRIPTION = "This goal has no description.";

	public delegate void QuestRequirementUpdatedEventHandler();
	public event QuestRequirementUpdatedEventHandler OnQuestRequirementUpdated;
	public void QuestRequirementUpdated() => OnQuestRequirementUpdated?.Invoke();

	public virtual string GetDescription()
	{
		return NO_DESCRIPTION;
	}

	protected virtual void AssignListener()
	{

	}

	public virtual bool IsComplete()
	{
		return false;
	}

	public virtual Transform TargetLocation()
	{
		return null;
	}
}
