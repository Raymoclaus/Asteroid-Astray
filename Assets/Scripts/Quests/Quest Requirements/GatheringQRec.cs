using UnityEngine;

public class GatheringQRec : QuestRequirement
{
	public Item.Type typeNeeded;
	public int amountNeeded = 1;
	private int currentAmount = 0;
	private string description;
	private string formattedDescription = "{0}: {1} / {2}";
	private bool formatDescription;

	public GatheringQRec(Item.Type typeNeeded, int amountNeeded, string description, bool formatDescription = true)
	{
		this.typeNeeded = typeNeeded;
		this.amountNeeded = amountNeeded;
		this.description = description.Replace(
			"#", amountNeeded.ToString()).Replace("?", Item.TypeName(typeNeeded));
		this.formatDescription = formatDescription;
	}

	public override void Activate()
	{
		base.Activate();
		GameEvents.OnItemCollected += EvaluateEvent;
	}

	private void EvaluateEvent(Item.Type type, int amount)
	{
		if (IsComplete() || !active) return;

		if (type == typeNeeded && amount != 0)
		{
			currentAmount += amount;
			QuestRequirementUpdated();
			if (currentAmount >= amountNeeded)
			{
				QuestRequirementCompleted();
				GameEvents.OnItemCollected -= EvaluateEvent;
			}
		}
	}

	public override string GetDescription() =>
		formatDescription ?
		string.Format(formattedDescription, description, currentAmount, amountNeeded)
		: description;

	public override Vector3? TargetLocation() => null;
}
