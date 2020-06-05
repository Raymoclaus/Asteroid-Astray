using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySystem
{
	public class CraftingRecipeStorage : MonoBehaviour
	{
		public List<CraftingRecipe> recipes;

		public CraftingRecipe FindMostValidRecipe(List<ItemStack> items)
		{
			SortedList<int, CraftingRecipe> validRecipes = FindValidRecipes(items);
			return validRecipes.Count != 0 ? validRecipes.Last().Value : null;
		}

		public SortedList<int, CraftingRecipe> FindValidRecipes(List<ItemStack> stacks)
		{
			SortedList<int, CraftingRecipe> validRecipes = new SortedList<int, CraftingRecipe>();

			for (int i = 0; i < recipes.Count; i++)
			{
				if (recipes[i].IsMatch(stacks))
				{
					validRecipes.Add(recipes[i].GetPriority(), recipes[i]);
				}
			}

			return validRecipes;
		}

		public int RecipeCount => recipes.Count;
	}
}
