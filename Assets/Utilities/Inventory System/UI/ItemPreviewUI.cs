using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class ItemPreviewUI : MonoBehaviour
	{
		[SerializeField] private Image image;
		[SerializeField] private TextMeshProUGUI itemName, description, flavourText;

		public void SetItemType(ItemObject type)
		{
			SetImage(type);
			SetItemName(type);
			SetDescription(type);
			SetFlavourText(type);
		}

		private void SetImage(ItemObject type)
		{
			image.sprite = Item.GetItemSprite(type);
			image.color = type == ItemObject.Blank ? Color.clear : Color.white;
		}

		private void SetItemName(ItemObject type)
		{
			itemName.text = Item.TypeName(type);
		}

		private void SetDescription(ItemObject type)
		{
			description.text = Item.Description(type);
		}

		private void SetFlavourText(ItemObject type)
		{
			flavourText.text = Item.FlavourText(type);
		}
	}
}