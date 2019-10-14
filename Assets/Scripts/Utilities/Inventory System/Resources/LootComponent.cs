using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public class LootComponent : MonoBehaviour
	{
		public List<Loot> lootStack;
		public ChasingItemPickup pickupPrefab;

		private void DropItem(Item.Type itemType)
		{
			ChasingItemPickup pickup = Instantiate(pickupPrefab);
			pickup.transform.position = transform.position;
			pickup.Pickup.SetItemType(itemType);
		}

		private void DropLoot(Loot loot)
		{
			ItemStack stack = loot.GetStack();
			int amount = stack.GetAmount();
			Item.Type itemType = stack.GetItemType();
			for (int i = 0; i < amount; i++)
			{
				DropItem(itemType);
			}
		}

		public void DropAllLoot()
		{
			for (int i = 0; i < lootStack.Count; i++)
			{
				DropLoot(lootStack[i]);
			}
		}
	}

}