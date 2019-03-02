using System.Collections.Generic;
using UnityEditor;

public static class Crafting
{
	public static CraftingRecipe? CheckRecipes(List<ItemStack> items)
	{
		string[] recipeGuids = AssetDatabase.FindAssets("t:CraftingRecipeSO");

		string[] recipePaths = new string[recipeGuids.Length];
		for (int i = 0; i < recipeGuids.Length; i++)
		{
			recipePaths[i] = AssetDatabase.GUIDToAssetPath(recipeGuids[i]);
		}

		CraftingRecipe? recipe = null;
		int priority = 0;
		for (int i = 0; i < recipePaths.Length; i++)
		{
			CraftingRecipe recipeCheck = AssetDatabase.LoadAssetAtPath<CraftingRecipeSO>(recipePaths[i]).recipe;
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
