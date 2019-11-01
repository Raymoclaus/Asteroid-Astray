using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class ItemPreviewUI : MonoBehaviour
	{
		[SerializeField] private Image image;
		[SerializeField] private TextMeshProUGUI itemName, description, flavourText;
		[SerializeField] private ItemSprites sprites;

		public void SetItemType(Item.Type type)
		{
			SetImage(type);
			SetItemName(type);
			SetDescription(type);
			SetFlavourText(type);
		}

		private void SetImage(Item.Type type)
		{
			image.sprite = sprites.GetItemSprite(type);
			image.color = type == Item.Type.Blank ? Color.clear : Color.white;
		}

		private void SetItemName(Item.Type type)
		{
			itemName.text = Item.TypeName(type);
		}

		private void SetDescription(Item.Type type)
		{
			description.text = Item.Description(type);
		}

		private void SetFlavourText(Item.Type type)
		{
			flavourText.text = Item.FlavourText(type);
		}
	}
}