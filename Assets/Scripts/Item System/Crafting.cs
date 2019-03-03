using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
}
