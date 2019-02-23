using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
	[SerializeField]
	private List<Image> slotSprites;
	private List<Image> itemSprites = new List<Image>();
	private List<Text> countTexts = new List<Text>();
	private int selected = 0;

	private ItemStack grabStack = new ItemStack(Item.Type.Blank, 0);
	private int originalGrabPos = -1;
	private bool grabbing = false;
	[SerializeField] private Transform grabTransform;
	[SerializeField] private Image grabImg;
	[SerializeField] private Text grabCountText;

	[SerializeField] private Camera cam;
	[SerializeField] private Shuttle shuttle;
	[SerializeField] private Inventory shuttleInventory;
	[SerializeField] private Image previewImg;
	[SerializeField] private Text previewName;
	[SerializeField] private Text previewDesc;
	[SerializeField] private Text previewFlav;
	[SerializeField] private ItemSprites sprites;


	private void Awake()
	{
		foreach (Image img in slotSprites)
		{
			itemSprites.Add(img.transform.GetChild(0).GetComponent<Image>());
			countTexts.Add(img.transform.GetChild(1).GetComponent<Text>());
		}
	}

	private void Update()
	{
		UpdateSlots();
		UpdateGrabUI();
	}

	private void UpdateSlots()
	{
		shuttle = shuttle ?? FindObjectOfType<Shuttle>();
		if (!shuttle) return;
		shuttleInventory = shuttleInventory ?? shuttle.storage;

		List<ItemStack> stacks = shuttleInventory.inventory;
		for (int i = 0; i < stacks.Count; i++)
		{
			Item.Type type = stacks[i].GetItemType();
			if (sprites)
			{
				itemSprites[i].sprite = sprites.GetItemSprite(type);
			}
			itemSprites[i].color = type == Item.Type.Blank ? Color.clear : Color.white;
			int count = stacks[i].GetAmount();
			countTexts[i].text = count <= 1 ? string.Empty : count.ToString();
		}

		Item.Type previewType = grabbing ? grabStack.GetItemType() : stacks[selected].GetItemType();
		if (sprites)
		{
			previewImg.sprite = sprites.GetItemSprite(previewType);
		}
		previewImg.color = previewType == Item.Type.Blank ? Color.clear : Color.white;
		previewName.text = Item.TypeName(previewType);
		previewDesc.text = Item.ItemDescription(previewType);
		previewFlav.text = Item.ItemFlavourText(previewType);
	}

	private void UpdateGrabUI()
	{
		if (!grabbing) return;
		Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
		pos.z = grabTransform.parent.position.z;
		grabTransform.position = pos;
		grabImg.sprite = sprites.GetItemSprite(grabStack.GetItemType());
		grabCountText.text = grabStack.GetAmount().ToString();
	}

	public void PointerEnter(Image slot)
	{
		UpdateSelected(slot);
	}

	public void PointerClick(Image slot)
	{

		if (grabbing)
		{
			int id = FindSlotId(slot);
			shuttleInventory.Swap(id, originalGrabPos);
			shuttleInventory.Insert(grabStack.GetItemType(), grabStack.GetAmount(), id);
			grabbing = false;
			grabStack.SetBlank();
			originalGrabPos = -1;
		}
		else if (shuttleInventory.inventory[selected].GetItemType() != Item.Type.Blank)
		{
			grabbing = true;
			grabStack.SetItemType(shuttleInventory.inventory[selected].GetItemType());
			grabStack.SetAmount(shuttleInventory.inventory[selected].GetAmount());
			shuttleInventory.inventory[selected].SetBlank();
			originalGrabPos = selected;
		}

		grabTransform.gameObject.SetActive(grabbing);
	}

	private void UpdateSelected(Image slot)
	{
		if (grabbing) return;

		int id = FindSlotId(slot);
		if (id >= 0 && id < slotSprites.Count
			&& shuttleInventory.inventory[id].GetItemType() != Item.Type.Blank)
		{
			selected = id;
		}
	}

	private int FindSlotId(Image slot)
	{
		for (int i = 0; i < slotSprites.Count; i++)
		{
			if (slotSprites[i] == slot)
			{
				return i;
			}
		}
		return -1;
	}
}