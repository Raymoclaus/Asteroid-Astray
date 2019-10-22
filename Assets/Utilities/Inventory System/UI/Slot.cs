using System;
using UnityEngine;

namespace InventorySystem.UI
{
	public class Slot : MonoBehaviour
	{
		[SerializeField] private ItemStackUI stackUI;
		private InventoryTab inventoryUI;
		public Inventory Inventory { get; private set; }
		public int ID { get; private set; }

		public void Initialise(InventoryTab uiController, Inventory inventory, int id)
		{
			inventoryUI = uiController;
			Inventory = inventory;
			ID = id;
			SetStack(inventory.ItemStacks[id]);
			Inventory.OnStackUpdated += UpdateStack;
		}

		private void UpdateStack(int index, Item.Type type, int amount)
		{
			if (index != ID) return;
			SetStack(type, amount);
		}

		public void SlotClicked()
		{
			inventoryUI.SlotClickDown(this);
		}

		public Item.Type ItemType
		{
			get => stackUI.ItemType;
			set => stackUI.ItemType = value;
		}

		public int Amount
		{
			get => stackUI.Amount;
			set => stackUI.Amount = value;
		}

		public void SetStack(ItemStack stack) => stackUI.SetStack(stack);

		public void SetStack(Item.Type type, int amount) => stackUI.SetStack(type, amount);

		public bool IsMaxed => stackUI.IsMaxed;
	}
}