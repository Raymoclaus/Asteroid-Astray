using System.Collections.Generic;

[System.Serializable]
public struct CraftingRecipe
{
	public List<CraftingRecipeIngredient> recipeList;
	public List<CraftingRecipeIngredient> result;
}

[System.Serializable]
public struct CraftingRecipeIngredient
{
	public Item.Type itemType;
	public int amount;
}