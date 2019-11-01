using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InventorySystem.UI
{
	public class RecipeTooltip : MonoBehaviour
	{
		[SerializeField] private ItemStackGroupUI ingredientsSide, resultsSide;
		[SerializeField] private TextMeshProUGUI tooltipHeader;
		[SerializeField] private CanvasGroup canvasGroup;

		private void Awake()
		{
			Hide();
		}

		public void SetText(string text)
		{
			tooltipHeader.text = text;
		}

		public void SetTextColour(Color col)
		{
			tooltipHeader.color = col;
		}

		public void SetRecipe(CraftingRecipe recipe)
		{
			List<ItemStack> ingredients = recipe.IngredientsCopy;
			List<ItemStack> results = recipe.ResultsCopy;
			SetIngredients(ingredients);
			SetResults(results);
		}

		public void SetIngredients(List<ItemStack> ingredients)
		{
			ingredientsSide.SetStackGroup(ingredients);
		}

		public void SetResults(List<ItemStack> results)
		{
			resultsSide.SetStackGroup(results);
		}

		public bool Hidden { get; private set; }

		public void Show()
		{
			canvasGroup.alpha = 1f;
			Hidden = false;
		}

		public void Hide()
		{
			canvasGroup.alpha = 0f;
			Hidden = true;
		}
	}
}