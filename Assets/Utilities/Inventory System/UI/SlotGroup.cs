using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.UI
{
	[System.Serializable]
	public class SlotGroup : MonoBehaviour
	{
		[SerializeField] private Slot slotPrefab;
		private List<Slot> slots = new List<Slot>();
		private Inventory inventory;
		[Range(0f, 1f)]
		public float alpha = 1f;
		private InventoryTab inventoryUI;

		public void SetInventory(Inventory inv)
		{
			inventory = inv;

			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < inv.ItemStacks.Count; i++)
			{
				Slot slot = Instantiate(slotPrefab, transform);
				slot.SetStack(inv.ItemStacks[i]);
				slots.Add(slot);
			}
		}

		public bool ContainsSlot(GameObject slotObj)
			=> slotObj.transform.IsChildOf(transform);

		public int SlotIndex(GameObject slotObj)
			=> ContainsSlot(slotObj) ? slotObj.transform.GetSiblingIndex() : -1;
	}
}