using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InventorySystem.UI
{
	public class ItemStackUI : MonoBehaviour
	{
		[SerializeField] private Image img;
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private ItemSprites sprites;
		private ItemStack stack = new ItemStack();

		public ItemStack StackCopy => new ItemStack(stack);

		public Item.Type ItemType
		{
			get => stack.GetItemType();
			set
			{
				stack.SetItemType(value);
				UpdateImage();
			}
		}

		public int Amount
		{
			get => stack.GetAmount();
			set
			{
				stack.SetAmount(value);
				UpdateText();
			}
		}

		public bool IsMaxed => stack.IsMaxed;

		public void SetStack(ItemStack newStack)
		{
			stack = newStack;
			UpdateImage();
			UpdateText();
		}

		private void UpdateImage()
		{
			if (ItemType == Item.Type.Blank)
			{
				img.enabled = false;
			}
			else
			{
				img.sprite = sprites.GetItemSprite(ItemType);
			}
		}

		private void UpdateText()
			=> text.text = Amount > 1 ? Amount.ToString() : string.Empty;
	}
}