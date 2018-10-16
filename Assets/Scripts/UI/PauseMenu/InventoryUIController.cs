﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
	[SerializeField]
	private List<Image> slotSprites;
	private List<Image> itemSprites = new List<Image>();
	private List<Text> countTexts = new List<Text>();
	[SerializeField]
	private Inventory shuttleInventory;
	private int selected = 0;
	[SerializeField]
	private Image previewImg;
	[SerializeField]
	private Text previewName;
	[SerializeField]
	private Text previewDesc;
	[SerializeField]
	private Text previewFlav;

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
	}

	private void UpdateSlots()
	{
		List<ItemStack> stacks = shuttleInventory.inventory;
		LoadedResources lr = GameController.GetResources();
		for (int i = 0; i < stacks.Count; i++)
		{
			Item.Type type = stacks[i].GetItemType();
			itemSprites[i].sprite = lr.GetItemSprite(type);
			itemSprites[i].color = type == Item.Type.Blank ? Color.clear : Color.white;
			int count = stacks[i].GetAmount();
			countTexts[i].text = count <= 1 ? string.Empty : count.ToString();
		}

		Item.Type previewType = stacks[selected].GetItemType();
		previewImg.sprite = lr.GetItemSprite(previewType);
		previewImg.color = previewType == Item.Type.Blank ? Color.clear : Color.white;
		previewName.text = previewType.ToString();
		previewDesc.text = Item.ItemDescription(previewType);
		previewFlav.text = Item.ItemFlavourText(previewType);
	}
}