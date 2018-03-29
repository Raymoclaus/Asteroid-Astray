using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
	private Item.Type type;
	private int amount;

	public ItemStack(Item.Type type, int num)
	{
		this.type = type;
		this.amount = num < 0 ? 0 : num;
	}

	public Item.Type GetItemType()
	{
		return type;
	}

	public void SetItemType(Item.Type newType)
	{
		type = newType;
	}

	public int GetAmount()
	{
		return amount;
	}

	public void SetAmount(int value)
	{
		if (value > 0)
		{
			amount = value % Item.StackLimit(type);
		}
		else
		{
			amount = 0;
			type = Item.Type.Blank;
		}
	}

	public bool AddAmount(int num)
	{
		if (num <= 0) return false;

		amount += num;
		if (amount > Item.StackLimit(type))
		{
			amount = Item.StackLimit(type);
		}
		return true;
	}

	public bool RemoveAmount(int num)
	{
		if (num > amount || num <= 0) return false;

		amount -= num;
		if (amount == 0)
		{
			type = Item.Type.Blank;
		}
		return true;
	}

	public override string ToString()
	{
		return string.Format("{0}x {1}", amount, type.ToString());
	}
}