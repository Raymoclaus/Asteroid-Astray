using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Crafting Recipe")]
	public class CraftingRecipe : ScriptableObject
	{
		[SerializeField] private List<ItemStack> recipeList, result;

		public List<ItemStack> IngredientsCopy => ItemStack.CreateCopyOfStacks(recipeList);
		public List<ItemStack> ResultsCopy => ItemStack.CreateCopyOfStacks(result);

		public int GetPriority()
		{
			int priority = 0;

			for (int i = 0; i < recipeList.Count; i++)
			{
				priority += GetPriority(recipeList[i]);
			}

			return priority;
		}

		public bool IsMatch(List<ItemStack> stacks)
		{
			for (int i = 0; i < recipeList.Count; i++)
			{
				ItemStack recipeStack = recipeList[i];
				bool foundMatch = false;
				for (int j = 0; j < stacks.Count; j++)
				{
					ItemStack stack = stacks[j];
					if (recipeStack.ItemType== stack.ItemType						&& recipeStack.Amount<= stack.Amount)
					{
						foundMatch = true;
						break;
					}
				}
				if (!foundMatch) return false;
			}
			return true;
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

		public int GetPriority(ItemStack stack)
			=> Item.TypeRarity(stack.ItemType) * stack.Amount;
	}
}