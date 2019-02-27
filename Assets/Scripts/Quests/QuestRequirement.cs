using UnityEngine;

public abstract class QuestRequirement
{
	private const string NO_DESCRIPTION = "This goal has no description.";
	protected bool completed = false;
	protected bool active = false;

	public delegate void QuestRequirementUpdatedEventHandler();
	public event QuestRequirementUpdatedEventHandler OnQuestRequirementUpdated;
	public void QuestRequirementUpdated() => OnQuestRequirementUpdated?.Invoke();

	public virtual string GetDescription() => NO_DESCRIPTION;

	public virtual bool IsComplete() => completed;

	public virtual Transform TargetLocation() => null;

	public virtual void Activate() => active = true;
}
