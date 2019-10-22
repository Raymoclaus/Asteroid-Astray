using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public class LootComponent : MonoBehaviour
	{
		public List<Loot> lootStack;
		public ChasingItemPickup pickupPrefab;

		private void DropItem(Item.Type itemType, IInventoryHolder inventoryHolder)
		{
			if (itemType == Item.Type.Blank) return;

			if (pickupPrefab == null)
			{
				Debug.Log("Pickup prefab is null", gameObject);
				return;
			}
			ChasingItemPickup pickup = Instantiate(pickupPrefab, transform.position,
				Quaternion.identity, ParticleGenerator.holder);
			pickup.Pickup.SetItemType(itemType);
			pickup.SetTarget(inventoryHolder);
		}

		private void DropLoot(Loot loot, IInventoryHolder inventoryHolder)
		{
			ItemStack stack = loot.GetStack();
			int amount = stack.Amount;
			Item.Type itemType = stack.ItemType;
			for (int i = 0; i < amount; i++)
			{
				DropItem(itemType, inventoryHolder);
			}
		}

		public void DropAllLoot(IInventoryHolder inventoryHolder)
		{
			for (int i = 0; i < lootStack.Count; i++)
			{
				DropLoot(lootStack[i], inventoryHolder);
			}
		}
	}

}