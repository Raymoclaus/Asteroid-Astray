using UnityEngine;

public class InteractionQReq : QuestRequirement
{
	private string description;
	private IActionTrigger interactableTrigger;
	private IInteractor actor;
	private Vector3? location;

	public InteractionQReq(IActionTrigger interactableTrigger, IInteractor actor, string description)
	{
		this.description = description;
		this.interactableTrigger = interactableTrigger;
		this.actor = actor;
	}

	public override void Activate()
	{
		base.Activate();
		interactableTrigger.OnInteracted += EvaluateEvent;
	}

	private void EvaluateEvent(IInteractor actor)
	{
		if (Completed || !active || this.actor != actor) return;

		QuestRequirementCompleted();
	}

	public override string GetDescription() => description;

	public override Vector3? TargetLocation() => interactableTrigger.PivotPosition;
}
