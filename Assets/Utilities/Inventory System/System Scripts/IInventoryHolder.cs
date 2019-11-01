using System;
using System.Collections.Generic;

namespace InventorySystem
{
	public interface IInventoryHolder
	{
		event Action<Item.Type, int> OnItemCollected, OnItemUsed;
		int GiveItem(Item.Type itemType);
		int GiveItem(ItemStack stack);
		Storage DefaultInventory { get; }
		List<Storage> GetAllInventories { get; }
		List<string> GetInventoryNames { get; }
		Storage GetInventoryByName(string inventoryName);
		Storage GetAppropriateInventory(Item.Type itemType);
		void AttachToInventoryUI();
	}
}