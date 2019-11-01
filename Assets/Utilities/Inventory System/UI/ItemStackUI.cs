using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class ItemStackUI : MonoBehaviour
	{
		[SerializeField] private Image img;
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private ItemSprites sprites;
		[SerializeField] private ItemStack stack = new ItemStack();

		public ItemStack StackCopy => new ItemStack(stack);

		public Item.Type ItemType
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

		public void SetStack(Item.Type type, int amount)
		{
			ItemType = type;
			Amount = amount;
		}

		private void UpdateImage()
		{
			img.enabled = ItemType != Item.Type.Blank;
			if (ItemType != Item.Type.Blank)
			{
				img.sprite = sprites.GetItemSprite(ItemType);
			}
		}

		private void UpdateText()
			=> text.text = Amount > 1 ? Amount.ToString() : string.Empty;
	}
}