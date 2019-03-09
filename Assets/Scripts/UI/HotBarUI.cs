using UnityEngine;
using UnityEngine.UI;

public class HotBarUI : MonoBehaviour
{
	[SerializeField] private Inventory inventory;
	[SerializeField] private ItemSprites sprites;
	[SerializeField] private Transform[] slots;
	private Image[] images;
	private Text[] texts;

	private void Awake()
	{
		images = new Image[slots.Length];
		texts = new Text[slots.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			images[i] = slots[i].GetChild(0).GetComponentInChildren<Image>();
			texts[i] = slots[i].GetChild(1).GetComponentInChildren<Text>();
		}
		UpdateSlots();
	}

	private void Update()
	{
		UpdateSlots();
	}

	private void UpdateSlots()
	{
		for (int i = 0; i < slots.Length && i < inventory.stacks.Count; i++)
		{
			ItemStack currentStack = inventory.stacks[i];
			images[i].sprite = sprites.GetItemSprite(currentStack.GetItemType());
			int amount = currentStack.GetAmount();
			texts[i].text = amount > 0 ? amount.ToString() : string.Empty;
			images[i].color = amount > 0 ? Color.white : Color.clear;
		}
	}
}
