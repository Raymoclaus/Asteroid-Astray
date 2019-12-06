using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class ItemStackUI : MonoBehaviour
	{
		[SerializeField] private Image img;
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private ItemStack stack = new ItemStack();

		public ItemStack StackCopy => new ItemStack(stack);

		public ItemObject ItemType
		{
			get => stack.ItemType;
			set
			{
				stack.ItemType = value;
				UpdateImage();
			}
		}

		public int Amount
		{
			get => stack.Amount;
			set
			{
				stack.Amount = value;
				UpdateText();
			}
		}

		public bool IsMaxed => stack.IsMaxed;

		public void SetStack(ItemStack newStack)
		{
			SetStack(newStack.ItemType, newStack.Amount);
		}

		public void SetStack(ItemObject type, int amount)
		{
			ItemType = type;
			Amount = amount;
		}

		private void UpdateImage()
		{
			img.enabled = ItemType != ItemObject.Blank;
			if (ItemType != ItemObject.Blank)
			{
				img.sprite = Item.GetItemSprite(ItemType);
			}
		}

		private void UpdateText()
			=> text.text = Amount > 1 ? Amount.ToString() : string.Empty;
	}
}