using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public int size = 10;
	public List<ItemStack> inventory = new List<ItemStack>();
	private bool initialised = false;

	protected virtual void Awake()
	{
		if (initialised) return;

		while (inventory.Count < size)
		{
			inventory.Add(new ItemStack(Item.Type.Blank, 0));
		}

		initialised = true;
	}

	public int ContainsItem(Item.Type type)
	{
		int amount = 0;
		foreach (ItemStack stack in inventory)
		{
			if (stack.GetItemType() == type)
			{
				amount += stack.GetAmount();
			}
		}
		return amount;
	}

	public int AddItem(Item.Type type, int num = 1, List<ItemStack> inv = null)
	{
		if (num <= 0) return -1;
		if (type == Item.Type.Blank) return 0;

		inv = inv != null ? inv : inventory;
		foreach (ItemStack stack in inv)
		{
			if (stack.GetItemType() == type)
			{
				int difference = Item.StackLimit(type) - stack.GetAmount();
				if (difference > 0)
				{
					int add = Math.Min(num, difference);
					num -= add;
					stack.AddAmount(add);
				}
			}
			if (num <= 0) return 0;
		}
		num = SetBlank(type, num, inv);
		return num;
	}

	private int SetBlank(Item.Type type, int num, List<ItemStack> inv = null)
	{
		inv = inv != null ? inv : inventory;
		foreach (ItemStack stack in inv)
		{
			if (stack.GetItemType() == Item.Type.Blank)
			{
				int add = Math.Min(num, Item.StackLimit(type));
				num -= add;
				stack.SetItemType(type);
				stack.AddAmount(add);
			}
			if (num <= 0) return 0;
		}

		return num;
	}

	public List<ItemStack> AddItems(List<ItemStack> items)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].GetItemType() == Item.Type.Blank) continue;
			items[i].SetAmount(AddItem(items[i].GetItemType(), items[i].GetAmount()));
		}
		return items;
	}

	public bool RemoveItem(Item.Type type, int num = 1)
	{
		if (num <= 0) return true;

		for (int i = inventory.Count - 1; i >= 0; i--)
		{
			if (inventory[i].GetItemType() == type)
			{
				int amount = inventory[i].GetAmount();
				int leftover = inventory[i].RemoveAmount(num);
				num -= amount - leftover;
			}
			if (num <= 0) return true;
		}

		return false;
	}

	public bool CanFit(List<ItemStack> items)
	{
		List<ItemStack> tempItems = new List<ItemStack>(items);
		List<ItemStack> tempInventory = new List<ItemStack>(inventory);

		for (int i = 0; i < tempItems.Count; i++)
		{
			if (AddItem(tempItems[i].GetItemType(), tempItems[i].GetAmount(), tempInventory) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public int Count(Item.Type? include = null, int minRarity = 0, int maxRarity = Item.MAX_RARITY)
	{
		int count = 0;
		bool fltr = include != null;

		if (include == Item.Type.Blank) return 0;

		foreach (ItemStack stack in inventory)
		{
			if (fltr && stack.GetItemType() != include) continue;
			int rarity = Item.TypeRarity(stack.GetItemType());
			if (rarity < minRarity && rarity > maxRarity) continue;

			count += stack.GetAmount();
		}
		return count;
	}

	public int[] CountRarities(Item.Type? exclude = null)
	{
		int[] counts = new int[Item.MAX_RARITY + 1];
		bool fltr = exclude != null;

		foreach (ItemStack stack in inventory)
		{
			if (fltr && stack.GetItemType() == exclude) continue;
			int rarity = Item.TypeRarity(stack.GetItemType());
			counts[rarity] += stack.GetAmount();
		}

		return counts;
	}

	public void RemoveByRarity(int rarity, int amount, Item.Type? exclude = null)
	{
		bool fltr = exclude != null;

		for (int i = inventory.Count - 1; i >= 0; i--)
		{
			Item.Type type = inventory[i].GetItemType();
			if (type == exclude) continue;

			if (Item.TypeRarity(type) == rarity)
			{
				int stackAmount = inventory[i].GetAmount();
				if (stackAmount > 0)
				{
					inventory[i].SetAmount(stackAmount - amount);
					amount -= stackAmount - inventory[i].GetAmount();
				}
			}

			if (amount <= 0) return;
		}

		if (amount > 0)
		{
			Debug.Log(string.Format("Unable to remove {0} items with {1} rarity.", amount, rarity));
		}
	}
}