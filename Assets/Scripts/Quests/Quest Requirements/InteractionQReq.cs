using UnityEngine;

public class InteractionQReq : QuestRequirement
{
	private string description;
	private InteractablePromptTrigger interactableTrigger;
	private Vector3? location;

	public InteractionQReq(InteractablePromptTrigger interactableTrigger, string description)
	{
		this.description = description;
		this.interactableTrigger = interactableTrigger;
	}

	public override void Activate()
	{
		base.Activate();
		interactableTrigger.OnInteraction += EvaluateEvent;
	}

	private void EvaluateEvent()
	{
		if (IsComplete() || !active) return;

		QuestRequirementCompleted();
	}

	public override string GetDescription()
	{
		return description;
	}

	public override Vector3? TargetLocation()
	{
		return interactableTrigger.transform.position;
	}
}
