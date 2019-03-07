﻿using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Crafting Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
	public CraftingRecipe recipe;

	public override string ToString()
	{
		return recipe.ToString();
	}
}