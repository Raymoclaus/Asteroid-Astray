using System.Linq;
using InventorySystem.UI;
using UnityEngine;
using TriggerSystem;

namespace InventorySystem
{
	public class ItemPickup : MonoBehaviour, IItemHolder
	{
		[SerializeField] private SpriteRenderer sprRend;

		private ItemObject itemType;
		public ItemObject ItemType
		{
			get => itemType;
			set
			{
				itemType = value;
				UpdateSprite();
			}
		}

		public void SetItemType(ItemObject type)
		{
			itemType = type;
			UpdateSprite();
		}

		private void UpdateSprite()
		{
			sprRend.sprite = Item.GetItemSprite(itemType);
		}

		public void SendItem(IInteractor interactor) => interactor.Interact(itemType);

		public void SendItem(IInventoryHolder inventoryHolder)
			=> inventoryHolder.GiveItem(itemType);
	}
}