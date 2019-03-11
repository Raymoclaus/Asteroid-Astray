using UnityEngine;

public class GatheringQRec : QuestRequirement
{
	public Item.Type typeNeeded;
	public int amountNeeded = 1;
	private int currentAmount = 0;
	private string description;
	private string formattedDescription = "{0}: {1} / {2}";

	public GatheringQRec(Item.Type typeNeeded, int amountNeeded, string description)
	{
		this.typeNeeded = typeNeeded;
		this.amountNeeded = amountNeeded;
		this.description = description.Replace(
			"#", amountNeeded.ToString()).Replace("?", Item.TypeName(typeNeeded));
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

	public override string GetDescription()
	{
		return string.Format(formattedDescription, description, currentAmount, amountNeeded);
	}

	public override Vector3? TargetLocation()
	{
		return null;
	}
}
