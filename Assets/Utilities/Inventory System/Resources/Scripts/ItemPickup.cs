using System.Linq;
using InventorySystem.UI;
using UnityEngine;
using TriggerSystem;

namespace InventorySystem
{
	public class ItemPickup : MonoBehaviour, IItemHolder
	{
		[SerializeField] private SpriteRenderer sprRend;

		private Item.Type itemType;
		public Item.Type ItemType
		{
			get => itemType;
			set
			{
				itemType = value;
				UpdateSprite();
			}
		}

		public void SetItemType(Item.Type type)
		{
			itemType = type;
			UpdateSprite();
		}

		private void UpdateSprite()
		{
			sprRend.sprite = Sprites.GetItemSprite(itemType);
		}

		private static ItemSprites sprites;
		private static ItemSprites Sprites
			=> sprites != null
				? sprites
				: (sprites = Resources.Load<ItemSprites>("Inventory System Scriptable Objects/ItemSpritesSO"));

		public void SendItem(IInteractor interactor) => interactor.Interact(itemType);

		public void SendItem(IInventoryHolder inventoryHolder)
			=> inventoryHolder.GiveItem(itemType);
	}
}