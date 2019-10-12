using System.Collections.Generic;

namespace InventorySystem
{
	[System.Serializable]
	public struct CraftingRecipe
	{
		public List<CraftingRecipeIngredient> recipeList;
		public List<CraftingRecipeIngredient> result;

		public int GetPriority()
		{
			int priority = 0;

			for (int i = 0; i < recipeList.Count; i++)
			{
				priority += recipeList[i].GetPriority();
			}

			return priority;
		}

		public bool IsMatch(List<ItemStack> stacks)
		{
			for (int i = 0; i < recipeList.Count; i++)
			{
				if (!recipeList[i].IsMatch(stacks)) return false;
			}
			return true;
		}

		public List<ItemStack> GetRecipeStacks()
		{
			List<ItemStack> stacks = new List<ItemStack>(recipeList.Count);
			for (int i = 0; i < recipeList.Count; i++)
			{
				stacks.Add(recipeList[i].GetStack());
			}
			return stacks;
		}

		public List<ItemStack> GetResultStacks()
		{
			List<ItemStack> stacks = new List<ItemStack>(result.Count);
			for (int i = 0; i < result.Count; i++)
			{
				stacks.Add(result[i].GetStack());
			}
			return stacks;
		}

		public override string ToString()
		{
			string s = string.Empty;
			for (int i = 0; i < recipeList.Count; i++)
			{
				s += $"{recipeList[i].ToString()}{(i == recipeList.Count - 1 ? " to make " : ", ")}";
			}

			for (int i = 0; i < result.Count; i++)
			{
				s += $"{result[i].ToString()}{(i == result.Count - 1 ? string.Empty : ", ")}";
			}
			return s;
		}
	}

	[System.Serializable]
	public struct CraftingRecipeIngredient
	{
		public Item.Type itemType;
		public int amount;

		public int GetPriority()
		{
			return Item.TypeRarity(itemType) * amount;
		}

		public bool IsMatch(List<ItemStack> stacks)
		{
			for (int i = 0; i < stacks.Count; i++)
			{
				if (stacks[i].GetItemType() == itemType && stacks[i].GetAmount() >= amount) return true;
			}
			return false;
		}

		public ItemStack GetStack()
		{
			return new ItemStack(itemType, amount);
		}

		public override string ToString()
		{
			return $"{amount}x {Item.TypeName(itemType)}";
		}
	}
}