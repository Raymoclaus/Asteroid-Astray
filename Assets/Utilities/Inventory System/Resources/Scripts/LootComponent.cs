using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public class LootComponent : MonoBehaviour
	{
		public List<Loot> lootStack;
		public ChasingItemPickup pickupPrefab;

		private static ItemDropper dropper;
		private static ItemDropper Dropper
			=> dropper != null ? dropper : (dropper = FindObjectOfType<ItemDropper>());

		private void DropItem(ItemObject itemType, IInventoryHolder target)
		{
			if (Dropper == null) return;
			Dropper.DropItem(itemType, transform.position, target);
		}

		private void DropLoot(Loot loot, IInventoryHolder target)
		{
			ItemStack stack = loot.GetStack();
			int amount = stack.Amount;
			ItemObject itemType = stack.ItemType;
			for (int i = 0; i < amount; i++)
			{
				DropItem(itemType, target);
			}
		}

		public void DropAllLoot(IInventoryHolder target)
		{
			for (int i = 0; i < lootStack.Count; i++)
			{
				DropLoot(lootStack[i], target);
			}
		}
	}

}