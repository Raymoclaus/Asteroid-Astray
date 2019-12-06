using System;
using System.Collections.Generic;

namespace InventorySystem
{
	public interface IInventoryHolder
	{
		event Action<ItemObject, int> OnItemCollected, OnItemUsed;
		int GiveItem(ItemObject itemType);
		int GiveItem(ItemStack stack);
		Storage DefaultInventory { get; }
		List<Storage> GetAllInventories { get; }
		List<string> GetInventoryNames { get; }
		Storage GetInventoryByName(string inventoryName);
		Storage GetAppropriateInventory(ItemObject itemType);
		void AttachToInventoryUI();
	}
}