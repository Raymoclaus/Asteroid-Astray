using UnityEngine;

namespace InventorySystem.UI
{
	public class Slot : MonoBehaviour
	{
		[SerializeField] private ItemStackUI stackUI;
		public Inventory Inventory { get; private set; }
		public int ID { get; private set; }

		public void Initialise(Inventory inventory, int id)
		{
			Inventory = inventory;
			ID = id;
			SetStack(inventory.ItemStacks[id]);
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

		public bool IsMaxed => stackUI.IsMaxed;
	}
}