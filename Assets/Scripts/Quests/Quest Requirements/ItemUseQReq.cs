using UnityEngine;

public class ItemUseQReq : QuestRequirement
{
	public Item.Type typeNeeded;
	public int amountNeeded = 1;
	private int currentAmount = 0;
	private string description;
	private const string formattedDescription = "{0}: {1} / {2}";

	public ItemUseQReq(Item.Type typeNeeded, int amountNeeded, string description)
	{
		this.typeNeeded = typeNeeded;
		this.description = description.Replace(
			"#", amountNeeded.ToString())
			.Replace("?", Item.TypeName(typeNeeded));
	}

	public override void Activate()
	{
		base.Activate();
		GameEvents.OnItemUsed += EvaluateEvent;
	}

	private void EvaluateEvent(Item.Type type)
	{
		if (IsComplete() || !active) return;

		if (type == typeNeeded)
		{
			currentAmount++;
			QuestRequirementUpdated();
			if (currentAmount >= amountNeeded)
			{
				QuestRequirementCompleted();
				GameEvents.OnItemUsed -= EvaluateEvent;
			}
		}
	}

	public override string GetDescription()
	{
		return string.Format(formattedDescription, description, currentAmount, amountNeeded);
	}

	public override Vector3? TargetLocation()
	{
		return base.TargetLocation();
	}
}
