using System.Linq;
using UnityEngine;
using TriggerSystem;

namespace InventorySystem
{
	using InventorySystem.UI;

	public class ItemPickup : MonoBehaviour, IItemHolder
	{
		[SerializeField] private SpriteRenderer sprRend;
		private static ItemSprites spritesAsset;

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
			GetItemSprites();
			if (spritesAsset != null)
			{
				sprRend.sprite = spritesAsset.GetItemSprite(itemType);
			}
		}

		private void GetItemSprites()
		{
			if (spritesAsset == null)
			{
				spritesAsset = Resources.LoadAll<ItemSprites>("").FirstOrDefault();
			}
		}

		public void SendItem(IInteractor interactor) => interactor.Interact(itemType);

		public void SendItem(IInventoryHolder inventoryHolder)
			=> inventoryHolder.GiveItem(itemType);
	}
}