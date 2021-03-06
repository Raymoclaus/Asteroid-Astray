﻿using System;
using System.Collections.Generic;

namespace InventorySystem
{
	public interface ICrafter : IInventoryHolder
	{
		event Action<List<ItemStack>> OnItemsCrafted;
		CraftingRecipeStorage GetRecipeStorage { get; }
		bool Craft(CraftingRecipe recipe);
		bool HasItems(List<ItemStack> stacks);
		bool CanCraftRecipe(CraftingRecipe recipe);
	}
}