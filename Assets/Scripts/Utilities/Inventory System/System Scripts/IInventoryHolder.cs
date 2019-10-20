using System;
using System.Collections.Generic;

namespace InventorySystem
{
	public interface IInventoryHolder
	{
		event Action<Item.Type, int> OnItemCollected, OnItemUsed;
		int GiveItem(Item.Type itemType);
		int GiveItem(ItemStack stack);
		Inventory DefaultInventory { get; }
		List<Inventory> GetAllInventories { get; }
		List<string> GetInventoryNames { get; }
		Inventory GetInventoryByName(string inventoryName);
		Inventory GetAppropriateInventory(Item.Type itemType);
		void AttachToInventoryUI();
	}
}