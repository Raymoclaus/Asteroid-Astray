using System;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;

namespace InventorySystem
{
	[Serializable]
	public class ItemStack
	{
		[SerializeField]
		private ItemObject type;
		[SerializeField]
		private int amount;

		public bool IsMaxed => amount == type.GetStackLimit();

		public ItemStack(ItemObject type, int amount)
		{
			this.type = amount <= 0 ? ItemObject.Blank : type;
			this.amount = type == ItemObject.Blank ? 0
				: Mathf.Min(Mathf.Max(1, amount), type.GetStackLimit());
		}

		public ItemStack(ItemObject type)
			: this(type, 1) { }

		public ItemStack(ItemStack stack)
			: this(stack.ItemType, stack.Amount) { }

		public ItemStack()
			: this(ItemObject.Blank) { }

		public ItemObject ItemType
		{
			get => type;
			set
			{
				type = value;
				if (type != ItemObject.Blank) return;
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
					amount = Mathf.Min(value, type.GetStackLimit());
				}
				else
				{
					amount = 0;
					type = ItemObject.Blank;
				}
			}
		}

		public int AddAmount(int num)
		{
			if (num <= 0) return num;

			amount += num;
			int leftOver = Mathf.Max(amount - type.GetStackLimit(), 0);
			if (amount > type.GetStackLimit())
			{
				amount = type.GetStackLimit();
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
				type = ItemObject.Blank;
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
		public static int AddItem(List<ItemStack> stacks, ItemObject type, int num,
			bool noLimit, Action<int, ItemObject, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
		{
			if (num <= 0) return 0;
			if (type == ItemObject.Blank) return 0;

			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType == type)
				{
					int difference = type.GetStackLimit() - stacks[i].Amount;
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
			bool noLimit, Action<int, ItemObject, int> stackUpdatedCallback,
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
			Action<int, ItemObject, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].ItemType == ItemObject.Blank) continue;
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
		private static int SetBlank(List<ItemStack> stacks, ItemObject type,
			int num, int maxSize, bool noLimit,
			Action<int, ItemObject, int> stackUpdatedCallback,
			Action<int, int> sizeChangedCallback)
		{
			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].ItemType == ItemObject.Blank)
				{
					int add = Math.Min(num, type.GetStackLimit());
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

		public static bool RemoveItemAtIndex(List<ItemStack> stacks, int index, int num = 1,
			Action<int, ItemObject, int> callback = null)
		{
			if (num <= 0) return true;
			if (index < 0 && index >= stacks.Count) return false;

			ItemStack stack = stacks[index];
			if (stack.ItemType == ItemObject.Blank) return false;
			if (stack.Amount == 0) return false;

			int expectedValue = stack.Amount - num;
			int result = stack.RemoveAmount(num);
			callback?.Invoke(index, stack.ItemType, result);

			return expectedValue == result;
		}

		public static bool RemoveItem(List<ItemStack> stacks, ItemObject type, int num = 1, Action<int, ItemObject, int> callback = null)
		{
			if (num <= 0) return true;

			for (int i = stacks.Count - 1; i >= 0; i--)
			{
				ItemObject stackType = stacks[i].ItemType;
				if (stackType == type)
				{
					int amount = stacks[i].Amount;
					RemoveItemAtIndex(stacks, i, num, callback);
					int leftover = stacks[i].Amount;
					num -= amount - leftover;
				}
				if (num <= 0) return true;
			}

			return false;
		}

		public static bool RemoveItem(List<ItemStack> stacks, ItemStack stack, Action<int, ItemObject, int> callback = null)
			=> RemoveItem(stacks, stack.ItemType, stack.Amount, callback);

		public static void RemoveItems(List<ItemStack> stacks, List<ItemStack> itemsToRemove, Action<int, ItemObject, int> callback = null)
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
			type = ItemObject.Blank;
			amount = 0;
		}

		public int Value => type.GetTypeRarity() * amount;

		/// <summary>
		/// Iterates through stacks and combines stacks of similar type if they are not at maximum limit.
		/// </summary>
		/// <param name="stacks"></param>
		public static void Simplify(List<ItemStack> stacks)
		{
			for (int i = 1; i < stacks.Count; i++)
			{
				ItemStack currentStack = stacks[i];
				if (currentStack.ItemType == ItemObject.Blank) continue;

				for (int j = 0; j < i; j++)
				{
					ItemStack priorStack = stacks[j];
					if (priorStack.ItemType == ItemObject.Blank) continue;

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
				if (stacks[i].ItemType == ItemObject.Blank)
				{
					stacks.RemoveAt(i);
				}
			}
		}

		public static bool CanFit(List<ItemStack> stacks, ItemStack itemsToFit)
		{
			ItemObject type = itemsToFit.ItemType;
			if (type == ItemObject.Blank) return true;
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
				ItemObject stackType = stack.ItemType;
				if (stackType == ItemObject.Blank) continue;
				int spaceForItem = SpaceLeftForItemType(stacks, stackType, false);
				int stackAmount = stack.Amount;
				int spillAmount = Mathf.Max(0, spaceForItem - stackAmount);
				if (spillAmount == 0) continue;

				for (int j = 0; j < spillingItems.Count; j++)
				{
					ItemStack spillStack = spillingItems[j];
					ItemObject spillType = spillStack.ItemType;
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

		public static int SpaceLeftForItemType(List<ItemStack> stacks, ItemObject type, bool includeEmptyStacks)
		{
			if (type == ItemObject.Blank) return 0;

			int count = 0;
			for (int i = 0; i < stacks.Count; i++)
			{
				if (!includeEmptyStacks && stacks[i].ItemType == ItemObject.Blank) continue;
				ItemObject stackType = stacks[i].ItemType;
				if (stackType != ItemObject.Blank && stackType != type) continue;
				int maxAmount = type.GetStackLimit();
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
				if (stacks[i].ItemType == ItemObject.Blank)
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

		/// <summary>
		/// Counts the number of items in a list.
		/// </summary>
		/// <param name="stacks"></param>
		/// <returns></returns>
		public static int Count(List<ItemStack> stacks)
		{
			int count = 0;

			for (int i = 0; i < stacks.Count; i++)
			{
				ItemStack stack = stacks[i];
				count += stack.Amount;
			}
			return count;
		}

		/// <summary>
		/// Counts the number of unique items in a list.
		/// </summary>
		/// <param name="stacks"></param>
		/// <returns></returns>
		public static int GetNumberOfUniqueItems(List<ItemStack> stacks)
		{
			HashSet<ItemObject> set = new HashSet<ItemObject>();

			foreach (ItemStack stack in stacks)
			{
				ItemObject itemType = stack.ItemType;
				if (itemType == ItemObject.Blank) continue;
				set.Add(stack.ItemType);
			}

			return set.Count;
		}

		/// <summary>
		/// Counts the number of items in a list with the same type as given type.
		/// </summary>
		/// <param name="stacks"></param>
		/// <param name="type"></param>
		/// <returns>Returns number of items found that meet the criteria.</returns>
		public static int Count(List<ItemStack> stacks, ItemObject type)
		{
			int count = 0;

			if (type == ItemObject.Blank) return 0;

			for (int i = 0; i < stacks.Count; i++)
			{
				ItemStack stack = stacks[i];
				if (stack.ItemType != type) continue;
				count += stack.Amount;
			}
			return count;
		}

		/// <summary>
		/// Counts number of items in a list within a rarity range. Inclusive
		/// </summary>
		/// <param name="stacks"></param>
		/// <param name="minRarity"></param>
		/// <param name="maxRarity"></param>
		/// <returns>Returns number of items found that meet the criteria.</returns>
		public static int Count(List<ItemStack> stacks, int minRarity, int maxRarity)
		{
			int count = 0;
			int min = Mathf.Min(minRarity, maxRarity);
			int max = Mathf.Max(minRarity, maxRarity);

			for (int i = min; i <= max; i++)
			{
				count += Count(stacks, i);
			}
			return count;
		}

		public static int Count(List<ItemStack> stacks, int rarity)
		{
			int count = 0;

			for (int i = 0; i < stacks.Count; i++)
			{
				ItemStack stack = stacks[i];
				if (rarity != stack.ItemType.GetTypeRarity()) continue;
				count += stack.Amount;
			}

			return count;
		}

		//expected format: "<amount>x<itemType>" spaces are removed automatically
		//example: "5xStone"
		public static bool TryParse(string toParse, out ItemStack result)
		{
			try
			{
				toParse = toParse.Replace(" ", string.Empty);
				string[] args = toParse.Split('x');
				int.TryParse(args[0], out int amount);
				ItemObject type = Item.GetItemByName(args[1]);
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