using System;
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

		public bool IsMaxed { get { return amount == Item.StackLimit(type); } }

		public ItemStack(Item.Type type, int num)
		{
			this.type = num <= 0 ? Item.Type.Blank : type;
			this.amount = num < 0 ? 0 : num;
		}

		public ItemStack(Item.Type type)
		{
			this.type = type;
			amount = 1;
		}

		public ItemStack() { }

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