﻿using System;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
	[SerializeField]
	private Item.Type type;
	[SerializeField]
	private int amount;

	public bool IsMaxed { get { return amount == Item.StackLimit(type); } }

	public ItemStack(Item.Type type, int num)
	{
		this.type = num <= 0 ? Item.Type.Blank : type;
		this.amount = num < 0 ? 0 : num;
	}

	public ItemStack()
	{

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
			amount = Mathf.Min(value, Item.StackLimit(type));
		}
		else
		{
			amount = 0;
			type = Item.Type.Blank;
		}
	}

	public int AddAmount(int num)
	{
		if (num <= 0) return num;

		amount += num;
		int leftOver = Mathf.Max(amount - Item.StackLimit(type), 0);
		if (amount > Item.StackLimit(type))
		{
			amount = Item.StackLimit(type);
		}
		return leftOver;
	}

	public int RemoveAmount(int num)
	{
		if (num < 0) return amount;

		amount -= num;
		if (amount <= 0)
		{
			amount = 0;
			type = Item.Type.Blank;
		}
		return amount;
	}

	public override string ToString()
	{
		return string.Format("{0}x {1}", amount, type.ToString());
	}

	public void SetBlank()
	{
		type = Item.Type.Blank;
		amount = 0;
	}

	public int GetValue()
	{
		return Item.TypeRarity(type) * amount;
	}
}