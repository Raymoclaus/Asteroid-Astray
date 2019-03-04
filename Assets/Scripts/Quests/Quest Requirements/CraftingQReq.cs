using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingQReq : QuestRequirement
{
	public Item.Type typeNeeded;
	public int amountNeeded = 1;
	private int currentAmount = 0;
	private string description;
	private string formattedDescription = "{0}: {1} / {2}";

	public CraftingQReq(Item.Type typeNeeded, int amountNeeded, string description)
	{
		this.typeNeeded = typeNeeded;
		this.description = description.Replace("#", amountNeeded.ToString()).Replace("?", Item.TypeName(typeNeeded));
	}

	public override void Activate()
	{
		base.Activate();
		GameEvents.OnItemCrafted += EvaluateEvent;
	}

	private void EvaluateEvent(Item.Type type, int amount)
	{
		if (completed || !active) return;
		
		if (type == typeNeeded)
		{
			currentAmount += amount;
			Debug.Log($"Quest requirement updated: {GetDescription()}");
			if (completed = currentAmount >= amountNeeded)
			{
				QuestRequirementUpdated();
				GameEvents.OnItemCrafted -= EvaluateEvent;
			}
		}
	}

	public override string GetDescription()
	{
		return string.Format(formattedDescription, description, currentAmount, amountNeeded);
	}

	public override bool IsComplete()
	{
		return base.IsComplete();
	}

	public override Transform TargetLocation()
	{
		return null;
	}
}
