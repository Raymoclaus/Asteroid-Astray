using System.Linq;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
	private Item.Type itemType;
	[SerializeField] private SpriteRenderer sprRend;
	private static ItemSprites spritesAsset;

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