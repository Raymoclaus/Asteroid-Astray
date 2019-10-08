using UnityEngine;

public class InteractionQReq : QuestRequirement
{
	private string description;
	private InteractablePromptTrigger interactableTrigger;
	private Triggerer actor;
	private Vector3? location;

	public InteractionQReq(InteractablePromptTrigger interactableTrigger, Triggerer actor, string description)
	{
		this.description = description;
		this.interactableTrigger = interactableTrigger;
		this.actor = actor;
	}

	public override void Activate()
	{
		base.Activate();
		interactableTrigger.OnInteraction += EvaluateEvent;
	}

	private void EvaluateEvent(Triggerer actor)
	{
		if (Completed || !active || this.actor != actor) return;

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
