using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemQReward : QuestReward
{
	public Item.Type type;
	public int amount;
	private string formattedString = "{0} (x{1})";

	public override string GetRewardName()
	{
		return string.Format(formattedString, Item.TypeName(type), amount);
	}

	public override void GiveReward(IQuester e)
	{
		e.ReceiveItemReward(type, amount);
	}
}
