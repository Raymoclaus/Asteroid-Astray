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
			image.sprite = type.GetItemSprite();
			image.color = type == ItemObject.Blank ? Color.clear : Color.white;
		}

		private void SetItemName(ItemObject type)
		{
			itemName.text = type.GetTypeName();
		}

		private void SetDescription(ItemObject type)
		{
			description.text = type.GetDescription();
		}

		private void SetFlavourText(ItemObject type)
		{
			flavourText.text = type.GetFlavourText();
		}
	}
}