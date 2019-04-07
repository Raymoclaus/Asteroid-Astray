using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUIObject : MonoBehaviour
{
	private Image img;
	private CraftingRecipe recipe;
	[SerializeField] private ItemSprites sprites;

	private void Awake()
	{
		img = GetComponent<Image>();
	}

	public void Initialise(CraftingRecipe recipe)
	{
		this.recipe = recipe;
		List<ItemStack> recipeResults = recipe.GetResultStacks();
	}
}
