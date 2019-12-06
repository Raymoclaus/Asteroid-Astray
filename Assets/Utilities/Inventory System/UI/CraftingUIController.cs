using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InventorySystem.UI
{
	public class CraftingUIController : MonoBehaviour
	{
		private ICrafter crafter;

		[SerializeField] private Transform recipeContent;
		[SerializeField] private RecipeUIObject recipeObjectPrefab;
		private List<RecipeUIObject> recipeObjects = new List<RecipeUIObject>();
		[SerializeField] private GameObject craftingDisabledObject;
		[SerializeField] private TextMeshProUGUI craftingDisabledReason;
		[SerializeField] private RecipeTooltip tooltip;

		public void SetCrafter(ICrafter newCrafter)
		{
			if (newCrafter == null) return;
			UnSubscribeFromCrafterInventories(crafter);
			crafter = newCrafter;
			SubscribeToCrafterInventories(crafter);
		}

		private void OnDestroy()
		{
			UnSubscribeFromCrafterInventories(crafter);
		}

		private void SubscribeToCrafterInventories(ICrafter crafter)
		{
			if (crafter == null) return;
			List<Storage> inventories = crafter.GetAllInventories;
			for (int i = 0; i < inventories.Count; i++)
			{
				inventories[i].OnStackUpdated += UpdateUI;
			}
		}

		private void UnSubscribeFromCrafterInventories(ICrafter crafter)
		{
			if (crafter == null) return;
			List<Storage> inventories = crafter.GetAllInventories;
			for (int i = 0; i < inventories.Count; i++)
			{
				inventories[i].OnStackUpdated -= UpdateUI;
			}
		}

		private void UpdateUI(int index, ItemObject type, int amount)
		{
			for (int i = 0; i < recipeObjects.Count; i++)
			{
				RecipeUIObject recipeObject = recipeObjects[i];
				recipeObject.UpdateCraftableIndicator();
				if (recipeObject != LastRecipeHovered) continue;
				recipeObject.UpdateTooltip(tooltip);
			}
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
				recipeObjects.Clear();

				//fill up recipe area with units currently accessible recipes
				List<CraftingRecipe> recipes = crafter.GetRecipeStorage.recipes;
				for (int i = 0; i < recipes.Count; i++)
				{
					RecipeUIObject recipeObject = Instantiate(
						recipeObjectPrefab, recipeContent);
					recipeObject.Initialise(this, recipes[i], crafter);
					recipeObjects.Add(recipeObject);
				}
			}
		}

		private void Update()
		{
			if (tooltip.Hidden || LastRecipeHovered == null) return;
			Vector3 lastRecipePosition = LastRecipeHovered.transform.position;
			tooltip.transform.position = lastRecipePosition;
		}

		private RecipeUIObject LastRecipeHovered { get; set; }

		public void OnHoverEnterRecipeObject(RecipeUIObject recipeUIObj)
		{
			LastRecipeHovered = recipeUIObj;
			recipeUIObj.UpdateTooltip(tooltip);
			tooltip.Show();
		}

		public void OnHoverExitRecipeObject(RecipeUIObject recipeUIObj)
		{
			tooltip.Hide();
		}
	}
}