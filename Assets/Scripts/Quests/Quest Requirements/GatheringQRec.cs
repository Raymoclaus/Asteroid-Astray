using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringQRec : QuestRequirement
{
	public Item.Type typeNeeded;
	public int amountNeeded = 1;
	private int currentAmount = 0;
	public string description;
	private string formattedDescription = "{0}: {1} / {2}";

	public override string GetDescription()
	{
		return string.Format(formattedDescription, description, currentAmount, amountNeeded);
	}

	public override bool IsComplete()
	{
		return currentAmount >= amountNeeded;
	}

	public override Transform TargetLocation()
	{
		return null;
	}

	protected override void AssignListener()
	{
		GameEvents.OnItemCollected += EvaluateEvent;
	}

	private void EvaluateEvent(Item.Type type, int amount)
	{
		if (type == typeNeeded)
		{
			currentAmount += amount;
			if (IsComplete())
			{
				QuestRequirementUpdated();
			}
		}
	}
}
