using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public class Inventory : MonoBehaviour
	{
		//index, type, new amount
		public event Action<int, Item.Type, int> OnStackUpdated;
		//old size, new size
		public event Action<int, int> OnSizeChanged;

		[SerializeField] private string inventoryName;
		[SerializeField] private int size = 10;
		[SerializeField] private bool noLimit = false;
		[SerializeField] private List<ItemStack> itemStacks = new List<ItemStack>();

		private void Awake()
		{
			TrimPadStacks();
		}

		public string InventoryName => inventoryName;

		public int Size => size;

		public List<ItemStack> ItemStacks
		{
			get => itemStacks;
			private set => itemStacks = value;
		}

		public void SetData(InventoryData data)
		{
			if (data.stacks != null)
			{
				size = data.size;
				ItemStacks = data.stacks;
			}
			TrimPadStacks();
		}

		public int AmountOfItem(Item.Type type)
		{
			int amount = 0;
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStack stack = ItemStacks[i];
				if (stack.ItemType== type)
				{
					amount += stack.Amount;
				}
			}
			return amount;
		}

		private int EmptySlotCount()
		{
			int count = 0;
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				if (ItemStacks[i].ItemType== Item.Type.Blank)
				{
					count++;
				}
			}
			return count;
		}

		public bool HasItems() => EmptySlotCount() < size;

		public int AddToStack(Item.Type itemType, int amount, int index)
		{
			if (index < 0 || index >= Size) return amount;
			ItemStack stack = ItemStacks[index];
			if (itemType != stack.ItemType) return amount;
			int leftOver = stack.AddAmount(amount);
			OnStackUpdated?.Invoke(index, itemType, stack.Amount);
			return leftOver;
		}

		public int AddItem(Item.Type type, int num = 1)
		{
			if (num <= 0) return 0;
			if (type == Item.Type.Blank) return 0;
			
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				if (ItemStacks[i].ItemType== type)
				{
					int difference = Item.StackLimit(type) - ItemStacks[i].Amount;
					if (difference > 0)
					{
						int add = Math.Min(num, difference);
						num -= add;
						ItemStacks[i].AddAmount(add);
						OnStackUpdated?.Invoke(i, type, ItemStacks[i].Amount);
					}
				}
				if (num <= 0) return 0;
			}
			num = SetBlank(type, num);
			if (num > 0)
			{
				Debug.Log("Inventory too full to add all items");
			}
			return num;
		}

		public int AddItem(ItemStack stack)
			=> AddItem(stack.ItemType, stack.Amount);

		public List<ItemStack> AddItems(List<ItemStack> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].ItemType== Item.Type.Blank) continue;
				int leftOver = AddItem(items[i]);
				items[i].Amount = leftOver;
			}
			return items;
		}

		/// <summary>
		/// Fills blank slots with items.
		/// </summary>
		/// <returns>Number of items not able to be added</returns>
		private int SetBlank(Item.Type type, int num)
		{
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				if (ItemStacks[i].ItemType== Item.Type.Blank)
				{
					int add = Math.Min(num, Item.StackLimit(type));
					num -= add;
					ItemStacks[i].ItemType = type;
					ItemStacks[i].AddAmount(add);
					OnStackUpdated?.Invoke(i, type, ItemStacks[i].Amount);
				}
				if (num <= 0) return 0;
				if (i == ItemStacks.Count - 1 && noLimit)
				{
					ItemStacks.Add(new ItemStack());
					int oldSize = size;
					size++;
					OnSizeChanged?.Invoke(oldSize, size);
				}
			}

			return num;
		}

		public bool RemoveItem(Item.Type type, int num = 1)
		{
			if (num <= 0) return true;

			for (int i = ItemStacks.Count - 1; i >= 0; i--)
			{
				if (ItemStacks[i].ItemType== type)
				{
					int amount = ItemStacks[i].Amount;
					int leftover = ItemStacks[i].RemoveAmount(num);
					OnStackUpdated?.Invoke(i, ItemStacks[i].ItemType, ItemStacks[i].Amount);
					num -= amount - leftover;
				}
				if (num <= 0) return true;
			}

			return false;
		}

		public bool RemoveItem(ItemStack stack)
			=> RemoveItem(stack.ItemType, stack.Amount);

		public void RemoveItems(List<ItemStack> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				RemoveItem(items[i].ItemType, items[i].Amount);
			}
		}

		public int SpaceLeftForItemType(Item.Type type, bool includeEmptyStacks)
		{
			int count = 0;
			for (int i = 0; i < Size; i++)
			{
				if (!includeEmptyStacks && ItemStacks[i].ItemType== Item.Type.Blank) continue;
				int maxAmount = Item.StackLimit(type);
				int leftOver = maxAmount - ItemStacks[i].Amount;
				count += leftOver;
			}
			return count;
		}

		public bool CanFit(List<ItemStack> items)
		{
			if (noLimit) return true;
			int emptySlotCount = EmptySlotCount();
			if (emptySlotCount >= NonBlankCount(items)) return true;
			if (NonBlankCount(items) == 1) return CanFit(items[0]);

			//check space for each item (excluding blank spaces)
			//if any items need to spill into blank spaces to fit, store the spill amount
			//if the total spill amount is less than the amount that can fit in blank spaces
			//then return true

			List<ItemStack> spillingItems = new List<ItemStack>();
			for (int i = 0; i < items.Count; i++)
			{
				ItemStack stack = items[i];
				Item.Type stackType = stack.ItemType;
				if (stackType == Item.Type.Blank) continue;
				int spaceForItem = SpaceLeftForItemType(stackType, false);
				int stackAmount = stack.Amount;
				int spillAmount = Mathf.Max(0, spaceForItem - stackAmount);
				if (spillAmount == 0) continue;

				for (int j = 0; j < spillingItems.Count; j++)
				{
					ItemStack spillStack = spillingItems[j];
					Item.Type spillType = spillStack.ItemType;
					if (spillType != stackType) continue;
					spillAmount = spillStack.AddAmount(spillAmount);
					if (spillAmount == 0) break;
				}

				while (spillAmount > 0)
				{
					ItemStack newSpillStack = new ItemStack(stackType, spillAmount);
					spillAmount -= newSpillStack.Amount;
					spillingItems.Add(newSpillStack);
				}
			}

			return emptySlotCount >= NonBlankCount(spillingItems);
		}

		public bool CanFit(ItemStack stack)
		{
			if (noLimit) return true;
			if (stack.ItemType== Item.Type.Blank) return true;
			int spaceForItem = SpaceLeftForItemType(stack.ItemType, true);
			return spaceForItem <= stack.Amount;
		}

		public int NonBlankCount(List<ItemStack> stacks)
		{
			int count = 0;
			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType== Item.Type.Blank) continue;
				count++;
			}
			return count;
		}

		public void Simplify(List<ItemStack> stacks)
		{
			for (int i = 1; i < stacks.Count; i++)
			{
				ItemStack currentStack = stacks[i];
				if (currentStack.ItemType== Item.Type.Blank) continue;

				for (int j = 0; j < i; j++)
				{
					ItemStack priorStack = stacks[j];
					if (priorStack.ItemType== Item.Type.Blank) continue;

					if (currentStack.ItemType== priorStack.ItemType)
					{
						int currentAmount = currentStack.Amount;
						int leftOver = priorStack.AddAmount(currentAmount);
						currentStack.Amount = currentAmount;

						if (currentStack.Amount== 0) break;
					}
				}
			}
		}

		private void RemoveBlanks(List<ItemStack> stacks)
		{
			for (int i = stacks.Count - 1; i >= 0; i--)
			{
				if (stacks[i].ItemType== Item.Type.Blank)
				{
					stacks.RemoveAt(i);
				}
			}
		}

		public int Count(Item.Type? include = null, int minRarity = 0, int maxRarity = Item.MAX_RARITY)
		{
			int count = 0;
			bool fltr = include != null;

			if (include == Item.Type.Blank) return 0;

			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStack stack = ItemStacks[i];
				if (fltr && stack.ItemType!= include) continue;
				int rarity = Item.TypeRarity(stack.ItemType);
				if (rarity < minRarity && rarity > maxRarity) continue;

				count += stack.Amount;
			}
			return count;
		}

		public bool SetStacks(List<ItemStack> newStacks)
		{
			if (newStacks.Count > size) return false;
			ItemStacks = newStacks;
			TrimPadStacks();
			return true;
		}

		private void TrimPadStacks()
		{
			if (ItemStacks == null)
			{
				ItemStacks = new List<ItemStack>();
			}

			while (ItemStacks.Count < size)
			{
				ItemStacks.Add(new ItemStack());
			}

			if (ItemStacks.Count > size)
			{
				ItemStacks.RemoveRange(size, ItemStacks.Count - size);
			}
		}

		public void ClearAll()
		{
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStacks[i].SetBlank();
			}
			TrimPadStacks();
		}

		public int[] CountRarities(Item.Type? exclude = null)
		{
			int[] counts = new int[Item.MAX_RARITY + 1];
			bool fltr = exclude != null;

			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStack stack = ItemStacks[i];
				if (fltr && stack.ItemType== exclude) continue;
				int rarity = Item.TypeRarity(stack.ItemType);
				counts[rarity] += stack.Amount;
			}

			return counts;
		}

		public void RemoveByRarity(int rarity, int amount, Item.Type? exclude = null)
		{
			for (int i = ItemStacks.Count - 1; i >= 0; i--)
			{
				Item.Type type = ItemStacks[i].ItemType;
				if (type == exclude) continue;

				if (Item.TypeRarity(type) == rarity)
				{
					int stackAmount = ItemStacks[i].Amount;
					if (stackAmount > 0)
					{
						ItemStacks[i].Amount = stackAmount - amount;
						amount -= stackAmount - ItemStacks[i].Amount;
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
			if (a < 0 || b < 0 || a >= ItemStacks.Count || b >= ItemStacks.Count || a == b) return;

			//Item.Type typeA = inventory[a].GetItemType();
			//int amountA = inventory[a].GetAmount();
			//Item.Type typeB = inventory[b].GetItemType();
			//int amountB = inventory[b].GetAmount();

			//inventory[a].SetItemType(typeB);
			//inventory[a].SetAmount(amountB);
			//inventory[b].SetItemType(typeA);
			//inventory[b].SetAmount(amountA);
			ItemStack temp = ItemStacks[a];
			ItemStacks[a] = ItemStacks[b];
			ItemStacks[b] = temp;
		}

		public bool Insert(Item.Type type, int amount, int place)
		{
			if (place < 0 || place >= ItemStacks.Count) return false;

			if (ItemStacks[place].ItemType== Item.Type.Blank)
			{
				ItemStacks[place].ItemType = type;
				ItemStacks[place].Amount = amount;
				return true;
			}
			else
			{
				bool forward = false;
				int i = place + 1;
				for (; i < ItemStacks.Count; i++)
				{
					if (ItemStacks[i].ItemType== Item.Type.Blank)
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
						if (ItemStacks[i].ItemType== Item.Type.Blank)
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
							ItemStacks[place].ItemType = type;
							ItemStacks[place].Amount = amount;
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
			ItemStack temp = ItemStacks[place];
			ItemStacks[place] = stack;
			OnStackUpdated?.Invoke(place, stack.ItemType, stack.Amount);
			return temp;
		}

		public int GetValue()
		{
			int value = 0;
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				value += ItemStacks[i].Value;
			}
			return value;
		}

		public int FirstInstanceId(Item.Type type)
		{
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				if (ItemStacks[i].ItemType== type) return i;
			}
			return -1;
		}

		public InventoryData GetInventoryData() => new InventoryData(ItemStacks, size);

		[Serializable]
		public struct InventoryData
		{
			public List<ItemStack> stacks;
			public int size;

			public InventoryData(List<ItemStack> stacks, int size)
			{
				this.stacks = stacks;
				this.size = size;
			}
		}
	}
}