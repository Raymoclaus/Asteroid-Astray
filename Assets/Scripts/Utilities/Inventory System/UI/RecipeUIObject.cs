using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.UI
{
	public class RecipeUIObject : MonoBehaviour
	{
		[SerializeField] private ItemStackUI stackPrefab;
		private CraftingRecipe recipe;

		public void Initialise(CraftingRecipe recipe)
		{
			this.recipe = recipe;

			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			List<ItemStack> results = recipe.Results;
			for (int i = 0; i < results.Count; i++)
			{
				ItemStackUI stackobj = Instantiate(stackPrefab, transform);
				stackobj.SetStack(results[i]);
			}
		}
	}
}