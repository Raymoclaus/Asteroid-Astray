using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public int size = 10;
	public List<ItemStack> stacks = new List<ItemStack>();
	private bool initialised = false;

	protected virtual void Awake()
	{
		if (initialised) return;
		TrimPadStacks();
		initialised = true;
	}

	public int ContainsItem(Item.Type type)
	{
		int amount = 0;
		for (int i = 0; i < stacks.Count; i++)
		{
			ItemStack stack = stacks[i];
			if (stack.GetItemType() == type)
			{
				amount += stack.GetAmount();
			}
		}
		return amount;
	}

	private int EmptySlotCount()
	{
		int count = 0;
		for (int i = 0; i < stacks.Count; i++)
		{
			if (stacks[i].GetItemType() == Item.Type.Blank)
			{
				count++;
			}
		}
		return count;
	}

	public bool HasItems()
	{
		return EmptySlotCount() < size;
	}

	public int AddItem(Item.Type type, int num = 1, List<ItemStack> inv = null)
	{
		if (num <= 0) return 0;
		if (type == Item.Type.Blank) return 0;

		inv = inv != null ? inv : stacks;
		for (int i = 0; i < inv.Count; i++)
		{
			ItemStack stack = inv[i];
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

	public List<ItemStack> AddItems(List<ItemStack> items)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].GetItemType() == Item.Type.Blank) continue;
			items[i].SetAmount(AddItem(items[i].GetItemType(), items[i].GetAmount()));
		}
		return items;
	}

	private int SetBlank(Item.Type type, int num, List<ItemStack> inv = null)
	{
		inv = inv != null ? inv : stacks;
		for (int i = 0; i < inv.Count; i++)
		{
			ItemStack stack = inv[i];
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

	public bool RemoveItem(Item.Type type, int num = 1)
	{
		if (num <= 0) return true;

		for (int i = stacks.Count - 1; i >= 0; i--)
		{
			if (stacks[i].GetItemType() == type)
			{
				int amount = stacks[i].GetAmount();
				int leftover = stacks[i].RemoveAmount(num);
				num -= amount - leftover;
			}
			if (num <= 0) return true;
		}

		return false;
	}

	public void RemoveItems(List<ItemStack> items)
	{
		for (int i = 0; i < items.Count; i++)
		{
			RemoveItem(items[i].GetItemType(), items[i].GetAmount());
		}
	}

	public bool CanFit(List<ItemStack> items)
	{
		if (EmptySlotCount() >= items.Count) return true;

		List<ItemStack> tempItems = new List<ItemStack>(items);
		List<ItemStack> tempInventory = new List<ItemStack>(stacks);

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

		for (int i = 0; i < stacks.Count; i++)
		{
			ItemStack stack = stacks[i];
			if (fltr && stack.GetItemType() != include) continue;
			int rarity = Item.TypeRarity(stack.GetItemType());
			if (rarity < minRarity && rarity > maxRarity) continue;

			count += stack.GetAmount();
		}
		return count;
	}

	public bool SetStacks(List<ItemStack> newStacks)
	{
		if (newStacks.Count > stacks.Count) return false;
		stacks = newStacks;
		TrimPadStacks();
		return true;
	}

	private void TrimPadStacks()
	{
		while (stacks.Count < size)
		{
			stacks.Add(new ItemStack());
		}

		if (stacks.Count > size)
		{
			stacks.RemoveRange(size, stacks.Count - size);
		}
	}

	public void ClearAll()
	{
		for (int i = 0; i < stacks.Count; i++)
		{
			stacks[i].SetBlank();
		}
		TrimPadStacks();
	}

	public int[] CountRarities(Item.Type? exclude = null)
	{
		int[] counts = new int[Item.MAX_RARITY + 1];
		bool fltr = exclude != null;

		for (int i = 0; i < stacks.Count; i++)
		{
			ItemStack stack = stacks[i];
			if (fltr && stack.GetItemType() == exclude) continue;
			int rarity = Item.TypeRarity(stack.GetItemType());
			counts[rarity] += stack.GetAmount();
		}

		return counts;
	}

	public void RemoveByRarity(int rarity, int amount, Item.Type? exclude = null)
	{
		for (int i = stacks.Count - 1; i >= 0; i--)
		{
			Item.Type type = stacks[i].GetItemType();
			if (type == exclude) continue;

			if (Item.TypeRarity(type) == rarity)
			{
				int stackAmount = stacks[i].GetAmount();
				if (stackAmount > 0)
				{
					stacks[i].SetAmount(stackAmount - amount);
					amount -= stackAmount - stacks[i].GetAmount();
				}
			}

			if (amount <= 0) return;
		}

		if (amount > 0)
		{
			Debug.Log(string.Format("Unable to remove {0} items with {1} rarity.", amount, rarity));
		}
	}

	public void Swap(int a, int b)
	{
		if (a < 0 || b < 0 || a >= stacks.Count || b >= stacks.Count || a == b) return;

		//Item.Type typeA = inventory[a].GetItemType();
		//int amountA = inventory[a].GetAmount();
		//Item.Type typeB = inventory[b].GetItemType();
		//int amountB = inventory[b].GetAmount();

		//inventory[a].SetItemType(typeB);
		//inventory[a].SetAmount(amountB);
		//inventory[b].SetItemType(typeA);
		//inventory[b].SetAmount(amountA);
		ItemStack temp = stacks[a];
		stacks[a] = stacks[b];
		stacks[b] = temp;
	}

	public bool Insert(Item.Type type, int amount, int place)
	{
		if (place < 0 || place >= stacks.Count) return false;
		
		if (stacks[place].GetItemType() == Item.Type.Blank)
		{
			stacks[place].SetItemType(type);
			stacks[place].SetAmount(amount);
			return true;
		}
		else
		{
			bool forward = false;
			int i = place + 1;
			for (; i < stacks.Count; i++)
			{
				if (stacks[i].GetItemType() == Item.Type.Blank)
				{
					forward = true;
					break;
				}
			}

			bool backward = false;
			if (!forward)
			{
				i = place - 1;
				for (; i >= 0; i--)
				{
					if (stacks[i].GetItemType() == Item.Type.Blank)
					{
						backward = true;
						break;
					}
				}
			}

			if (!forward && !backward)
			{
				return false;
			}
			else
			{
				for (; ; i += forward ? -1 : 1)
				{
					if (i == place)
					{
						stacks[place].SetItemType(type);
						stacks[place].SetAmount(amount);
						break;
					}
					Swap(i, i + (forward ? -1 : 1));
				}
				return true;
			}
		}
	}

	public ItemStack Replace(ItemStack stack, int place)
	{
		ItemStack temp = stacks[place];
		stacks[place] = stack;
		return temp;
	}

	public int GetValue()
	{
		int value = 0;
		for (int i = 0; i < stacks.Count; i++)
		{
			value += stacks[i].GetValue();
		}
		return value;
	}
}