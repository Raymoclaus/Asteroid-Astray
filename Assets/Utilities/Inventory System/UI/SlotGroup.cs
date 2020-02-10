﻿using System;
using System.Collections.Generic;
using TabbedMenuSystem;
using UnityEngine;

namespace InventorySystem.UI
{
	[Serializable]
	public class SlotGroup : MonoBehaviour
	{
		[SerializeField] private Slot slotPrefab;
		private List<Slot> slots = new List<Slot>();
		private InventoryTab inventoryUI;
		private Storage inventory;
		[Range(0f, 1f)]
		public float alpha = 1f;

		public void Initialise(InventoryTab uiController, Storage inv)
		{
			inventoryUI = uiController;
			inventory = inv;

			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < inv.ItemStacks.Count; i++)
			{
				Slot slot = Instantiate(slotPrefab, transform);
				slot.Initialise(inventoryUI, inventory, i);
				slots.Add(slot);
			}
		}

		public bool ContainsSlot(GameObject slotObj)
			=> slotObj.transform.IsChildOf(transform);

		public int SlotIndex(GameObject slotObj)
			=> ContainsSlot(slotObj) ? slotObj.transform.GetSiblingIndex() : -1;
	}
}