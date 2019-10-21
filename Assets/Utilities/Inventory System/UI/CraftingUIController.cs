using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InventorySystem.UI
{
	public class CraftingUIController : MonoBehaviour
	{
		private ICrafter crafter;

		[SerializeField] private Transform recipeContent;
		[SerializeField] private RecipeUIObject recipeObjectPrefab;
		[SerializeField] private GameObject craftingDisabledObject;
		[SerializeField] private TextMeshProUGUI craftingDisabledReason;

		public void SetCrafter(ICrafter newCrafter)
		{
			if (newCrafter == null) return;
			crafter = newCrafter;
		}

		public void Setup()
		{
			//disable crafting UI if these conditions are met
			if (crafter == null || crafter.GetRecipeStorage == null)
			{
				craftingDisabledObject.SetActive(true);
				recipeContent.gameObject.SetActive(false);
				craftingDisabledReason.text = "Unit cannot craft";
			}
			else if (crafter.GetRecipeStorage.RecipeCount == 0)
			{
				craftingDisabledObject.SetActive(true);
				recipeContent.gameObject.SetActive(false);
				craftingDisabledReason.text = "Unit has no crafting recipes";
			}
			else
			{
				craftingDisabledObject.SetActive(false);
				recipeContent.gameObject.SetActive(true);

				foreach (Transform child in recipeContent)
				{
					Destroy(child.gameObject);
				}

				//fill up recipe area with units currently accessible recipes
				List<CraftingRecipe> recipes = crafter.GetRecipeStorage.recipes;

				for (int i = 0; i < recipes.Count; i++)
				{
					RecipeUIObject recipeObject = Instantiate(
						recipeObjectPrefab, recipeContent);
					recipeObject.Initialise(recipes[i]);
				}
			}
		}
	}
}