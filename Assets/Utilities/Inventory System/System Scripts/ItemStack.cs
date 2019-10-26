using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	[Serializable]
	public class ItemStack
	{
		[SerializeField]
		private Item.Type type;
		[SerializeField]
		private int amount;

		public bool IsMaxed => amount == Item.StackLimit(type);

		public ItemStack(Item.Type type, int amount)
		{
			this.type = amount <= 0 ? Item.Type.Blank : type;
			this.amount = type == Item.Type.Blank ? 0
				: Mathf.Min(Mathf.Max(1, amount), Item.StackLimit(type));
		}

		public ItemStack(Item.Type type)
			: this(type, 1) { }

		public ItemStack(ItemStack stack)
			: this(stack.ItemType, stack.Amount) { }

		public ItemStack()
			: this(Item.Type.Blank) { }

		public Item.Type ItemType
		{
			get => type;
			set
			{
				type = value;
				if (type != Item.Type.Blank) return;
				amount = 0;
			}
		}

		public int Amount
		{
			get => amount;
			set
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

		/// <summary>
		/// Increments any non-maxed stacks of equivalent item type by amount given.
		/// If no stacks of equivalent type are found, the items are placed in blank slots.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="num"></param>
		/// <returns>Returns number of items left over, due to not having room for them.</returns>
		public static int AddItem(List<ItemStack> stacks, Item.Type type, int num,
			bool noLimit, Action<int, Item.Type, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
		{
			if (num <= 0) return 0;
			if (type == Item.Type.Blank) return 0;

			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType == type)
				{
					int difference = Item.StackLimit(type) - stacks[i].Amount;
					if (difference > 0)
					{
						int add = Math.Min(num, difference);
						num -= add;
						stacks[i].AddAmount(add);
						stackUpdatedCallback?.Invoke(i, type, stacks[i].Amount);
					}
				}
				if (num <= 0) return 0;
			}
			num = SetBlank(stacks, type, num, stacks.Count, noLimit,
				stackUpdatedCallback, sizeChangedCallback);
			if (num > 0)
			{
				Debug.Log($"Stacks too full to add {num}x {type}.");
			}
			return num;
		}

		/// <summary>
		/// Increments any non-maxed stacks of equivalent item type by amount given.
		/// If no stacks of equivalent type are found, the items are placed in blank slots.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="num"></param>
		/// <returns>Returns number of items left over, due to not having room for them.</returns>
		public static int AddItem(List<ItemStack> stacks, ItemStack stack,
			bool noLimit, Action<int, Item.Type, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
			=> AddItem(stacks, stack.ItemType, stack.Amount, noLimit,
				stackUpdatedCallback, sizeChangedCallback);

		/// <summary>
		/// Adds item stacks to inventory.
		/// </summary>
		/// <param name="items"></param>
		/// <returns>Returns list of stacks that could not be added, due to not having room for them.</returns>
		public static List<ItemStack> AddItems(List<ItemStack> stacks,
			List<ItemStack> items, bool noLimit,
			Action<int, Item.Type, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].ItemType == Item.Type.Blank) continue;
				int leftOver = AddItem(stacks, items[i], noLimit,
				stackUpdatedCallback, sizeChangedCallback);
				items[i].Amount = leftOver;
			}
			RemoveBlanks(items);
			return items;
		}

		/// <summary>
		/// Fills blank slots with items.
		/// </summary>
		/// <returns>Number of items not able to be added</returns>
		private static int SetBlank(List<ItemStack> stacks, Item.Type type,
			int num, int maxSize, bool noLimit,
			Action<int, Item.Type, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
		{
			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType == Item.Type.Blank)
				{
					int add = Math.Min(num, Item.StackLimit(type));
					num -= add;
					stacks[i].ItemType = type;
					stacks[i].AddAmount(add);
					stackUpdatedCallback?.Invoke(i, type, stacks[i].Amount);
				}
				if (num <= 0) return 0;
				if (i == stacks.Count - 1 && noLimit)
				{
					int oldSize = stacks.Count;
					stacks.Add(new ItemStack());
					int newSize = stacks.Count;
					sizeChangedCallback?.Invoke(oldSize, newSize);
				}
			}

			return num;
		}

		public static bool RemoveItem(List<ItemStack> stacks, Item.Type type, int num = 1, Action<int, Item.Type, int> callback = null)
		{
			if (num <= 0) return true;

			for (int i = stacks.Count - 1; i >= 0; i--)
			{
				Item.Type stackType = stacks[i].ItemType;
				if (stackType == type)
				{
					int amount = stacks[i].Amount;
					int leftover = stacks[i].RemoveAmount(num);
					callback?.Invoke(i, stackType, stacks[i].Amount);
					num -= amount - leftover;
				}
				if (num <= 0) return true;
			}

			return false;
		}

		public static bool RemoveItem(List<ItemStack> stacks, ItemStack stack, Action<int, Item.Type, int> callback = null)
			=> RemoveItem(stacks, stack.ItemType, stack.Amount, callback);

		public static void RemoveItems(List<ItemStack> stacks, List<ItemStack> itemsToRemove, Action<int, Item.Type, int> callback = null)
		{
			for (int i = 0; i < itemsToRemove.Count; i++)
			{
				RemoveItem(stacks, itemsToRemove[i].ItemType, itemsToRemove[i].Amount, callback);
			}
		}

		public override string ToString()
			=> string.Format("{0}x {1}", amount, type.ToString());

		public void SetBlank()
		{
			type = Item.Type.Blank;
			amount = 0;
		}

		public int Value => Item.TypeRarity(type) * amount;

		/// <summary>
		/// Iterates through stacks and combines stacks of similar type if they are not at maximum limit.
		/// </summary>
		/// <param name="stacks"></param>
		public static void Simplify(List<ItemStack> stacks)
		{
			for (int i = 1; i < stacks.Count; i++)
			{
				ItemStack currentStack = stacks[i];
				if (currentStack.ItemType == Item.Type.Blank) continue;

				for (int j = 0; j < i; j++)
				{
					ItemStack priorStack = stacks[j];
					if (priorStack.ItemType == Item.Type.Blank) continue;

					if (currentStack.ItemType == priorStack.ItemType)
					{
						int currentAmount = currentStack.Amount;
						int leftOver = priorStack.AddAmount(currentAmount);
						currentStack.Amount = currentAmount;

						if (currentStack.Amount == 0) break;
					}
				}
			}
		}

		public static void RemoveBlanks(List<ItemStack> stacks)
		{
			for (int i = stacks.Count - 1; i >= 0; i--)
			{
				if (stacks[i].ItemType == Item.Type.Blank)
				{
					stacks.RemoveAt(i);
				}
			}
		}

		public static bool CanFit(List<ItemStack> stacks, ItemStack itemsToFit)
		{
			Item.Type type = itemsToFit.ItemType;
			if (type == Item.Type.Blank) return true;
			if (GetEmptyCount(stacks) > 0) return true;
			int spaceForitem = SpaceLeftForItemType(stacks, type, true);
			return spaceForitem >= itemsToFit.Amount;
		}

		public static bool CanFit(List<ItemStack> stacks, List<ItemStack> itemsToFit)
		{
			int emptySlotCount = GetEmptyCount(stacks);
			if (emptySlotCount >= GetNonEmptyCount(itemsToFit)) return true;
			if (GetNonEmptyCount(itemsToFit) == 1) return CanFit(stacks, itemsToFit[0]);

			//check space for each item (excluding blank spaces)
			//if any items need to spill into blank spaces to fit, store the spill amount
			//if the total spill amount is less than the amount that can fit in blank spaces
			//then return true

			List<ItemStack> spillingItems = new List<ItemStack>();
			for (int i = 0; i < itemsToFit.Count; i++)
			{
				ItemStack stack = itemsToFit[i];
				Item.Type stackType = stack.ItemType;
				if (stackType == Item.Type.Blank) continue;
				int spaceForItem = SpaceLeftForItemType(stacks, stackType, false);
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

			return emptySlotCount >= GetNonEmptyCount(spillingItems);
		}

		public static int SpaceLeftForItemType(List<ItemStack> stacks, Item.Type type, bool includeEmptyStacks)
		{
			int count = 0;
			for (int i = 0; i < stacks.Count; i++)
			{
				if (!includeEmptyStacks && stacks[i].ItemType == Item.Type.Blank) continue;
				Item.Type stackType = stacks[i].ItemType;
				if (stackType != Item.Type.Blank && stackType != type) continue;
				int maxAmount = Item.StackLimit(type);
				int leftOver = maxAmount - stacks[i].Amount;
				count += leftOver;
			}
			return count;
		}

		public static int GetEmptyCount(List<ItemStack> stacks)
		{
			int count = 0;
			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType == Item.Type.Blank)
				{
					count++;
				}
			}
			return count;
		}

		public static int GetNonEmptyCount(List<ItemStack> stacks)
			=> stacks.Count - GetEmptyCount(stacks);

		public static List<ItemStack> CreateCopyOfStacks(List<ItemStack> stacks)
		{
			List<ItemStack> copy = new List<ItemStack>();
			for (int i = 0; i < stacks.Count; i++)
			{
				copy.Add(CreateCopyOfStack(stacks[i]));
			}
			return copy;
		}

		public static ItemStack CreateCopyOfStack(ItemStack stack)
			=> new ItemStack(stack.ItemType, stack.Amount);

		public static int Count(List<ItemStack> stacks, Item.Type? include = null, int minRarity = Item.MIN_RARITY, int maxRarity = Item.MAX_RARITY)
		{
			int count = 0;
			bool fltr = include != null;

			if (include == Item.Type.Blank) return 0;

			for (int i = 0; i < stacks.Count; i++)
			{
				ItemStack stack = stacks[i];
				if (fltr && stack.ItemType != include) continue;
				int rarity = Item.TypeRarity(stack.ItemType);
				if (rarity < minRarity && rarity > maxRarity) continue;

				count += stack.Amount;
			}
			return count;
		}

		public static bool TryParse(string toParse, out ItemStack result)
		{
			try
			{
				toParse = toParse.Replace(" ", string.Empty);
				string[] args = toParse.Split('x');
				int amount;
				Item.Type type;
				int.TryParse(args[0], out amount);
				Enum.TryParse(args[1], out type);
				result = new ItemStack(type, amount);
				return true;
			}
			catch (ArgumentException e)
			{
				Debug.LogError(e);
				result = new ItemStack();
				return false;
			}
			catch (IndexOutOfRangeException e)
			{
				Debug.LogError(e);
				result = new ItemStack();
				return false;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				result = new ItemStack();
				return false;
			}
		}
	}
}