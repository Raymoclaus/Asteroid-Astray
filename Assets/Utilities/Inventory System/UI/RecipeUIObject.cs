using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class RecipeUIObject : MonoBehaviour
	{
		private enum State { Available, NotEnoughMaterials, InventoryFull }

		[SerializeField] private ItemStackGroupUI stackGroup;
		private CraftingUIController craftingUI;
		private CraftingRecipe recipe;
		private List<ItemStack> ingredients, results;
		private ICrafter crafter;
		[SerializeField] private Image craftableIndicator;
		private State currentState;
		[SerializeField] private string craftableString = "Available",
			uncraftableString = "Not Enough Materials",
			fullInventoryString = "Inventory Too Full";
		[SerializeField] private Color craftableColor = Color.green,
			uncraftableColor = Color.red,
			fullInventoryColor = Color.yellow;

		public void Initialise(CraftingUIController craftingUI, CraftingRecipe recipe, ICrafter crafter)
		{
			this.craftingUI = craftingUI;
			this.recipe = recipe;
			this.crafter = crafter;

			ingredients = recipe.IngredientsCopy;
			results = recipe.ResultsCopy;

			stackGroup.SetStackGroup(results);

			UpdateCraftableIndicator();
		}

		public void OnClicked()
		{
			crafter.Craft(recipe);
		}

		public void UpdateCraftableIndicator()
		{
			if (!crafter.HasItems(ingredients))
			{
				currentState = State.NotEnoughMaterials;
			}
			else
			{
				if (crafter.CanCraftRecipe(recipe))
				{
					currentState = State.Available;
				}
				else
				{
					currentState = State.InventoryFull;
				}
			}
			SetCraftableIndicatorColour(GetColor(currentState));
		}

		private void SetCraftableIndicatorColour(Color col)
			=> craftableIndicator.color = col;

		public void OnHoverEnter()
		{
			craftingUI.OnHoverEnterRecipeObject(this);
		}

		public void OnHoverExit()
		{
			craftingUI.OnHoverExitRecipeObject(this);
		}

		public void UpdateTooltip(RecipeTooltip tooltip)
		{
			tooltip.SetIngredients(ingredients);
			tooltip.SetRecipe(recipe);
			tooltip.SetText(GetTooltipText(currentState));
			tooltip.SetTextColour(GetColor(currentState));
		}

		private string GetTooltipText(State state)
		{
			switch (state)
			{
				default: return string.Empty;
				case State.Available: return craftableString;
				case State.NotEnoughMaterials: return uncraftableString;
				case State.InventoryFull: return fullInventoryString;
			}
		}

		private Color GetColor(State state)
		{
			switch (state)
			{
				default: return Color.white;
				case State.Available: return craftableColor;
				case State.NotEnoughMaterials: return uncraftableColor;
				case State.InventoryFull: return fullInventoryColor;
			}
		}

		public ItemObject MainItem => results[0].ItemType;
	}
}