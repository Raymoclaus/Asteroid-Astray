using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public static class Crafting
	{
		public static CraftingRecipe? CheckRecipes(List<ItemStack> items)
		{
			CraftingRecipeSO[] recipes = Resources.LoadAll<CraftingRecipeSO>(string.Empty);

			CraftingRecipe? recipe = null;
			int priority = 0;
			for (int i = 0; i < recipes.Length; i++)
			{
				CraftingRecipe recipeCheck = recipes[i].recipe;
				int checkPriority = recipeCheck.GetPriority();
				if (checkPriority > priority && recipeCheck.IsMatch(items))
				{
					recipe = recipeCheck;
					priority = checkPriority;
				}
			}

			return recipe;
		}

		public static CraftingRecipe? GetRecipeByName(string recipeName)
		{
			CraftingRecipeSO recipeSO = Resources.Load<CraftingRecipeSO>(recipeName);
			return recipeSO?.recipe;
		}
	}

}