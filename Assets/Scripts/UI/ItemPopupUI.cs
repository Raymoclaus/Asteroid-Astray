﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : PopupUI
{
	private List<ItemPopupObject> activePopups = new List<ItemPopupObject>();
	private List<ItemPopupObject> inactivePopups = new List<ItemPopupObject>();
	private List<PopupData> popupsToShow = new List<PopupData>();
	public Color textColor;
	[SerializeField]
	private float textFadeSpeed = 0.17f;
	private float xPos;
	[SerializeField]
	private ItemSprites sprites;

	private void Awake()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			RectTransform popup = (RectTransform)transform.GetChild(i);
			ItemPopupObject po = new ItemPopupObject(sprites,
				popup,
				popup.GetComponent<Image>(),
				popup.GetChild(0).GetComponent<Image>(),
				popup.GetChild(1).GetChild(0).GetComponent<Text>(),
				popup.GetChild(1).GetChild(1).GetComponent<Text>());
			inactivePopups.Add(po);
		}
		popupHeight = ((RectTransform)transform.GetChild(0)).rect.height;
		xPos = ((RectTransform)transform.GetChild(0)).anchoredPosition.x;
	}

	private void Update()
	{
		if (loadingTrackerSO.isLoading) return;

		while (popupsToShow.Count > 0)
		{
			if (activePopups.Count == popupViewLimit)
			{
				scrollDelayTimer += recordingModeTrackerSO.UnscaledDeltaTime;
				if (scrollDelayTimer >= scrollDelay)
				{
					RemovePopup(activePopups.Count - 1);
					for (int i = 0; i < activePopups.Count; i++)
					{
						activePopups[i].SetTimer(Mathf.Max(0f, activePopups[i].timer - scrollDelayTimer));
					}
					scrollDelayTimer = 0f;
				}
				else
				{
					break;
				}
			}

			if (activePopups.Count < popupViewLimit)
			{
				ItemPopupObject po = inactivePopups[0];
				activePopups.Insert(0, po);
				inactivePopups.RemoveAt(0);
				po.transform.anchoredPosition =
					new Vector2(xPos, -popupHeight / 2f - popupViewLimit * (popupHeight - 1));
				po.SetTimer(0f);
				po.amount = popupsToShow[0].amount;
				po.UpdateData(popupsToShow[0].type);
				popupsToShow.RemoveAt(0);
				po.UIimg.material.SetFloat("_Radius", 0f);
			}
		}

		for (int i = 0; i < activePopups.Count; i++)
		{
			ItemPopupObject po = activePopups[i];
			po.UIimg.material.SetFloat("_Flash", 1f);
			float delta = po.UIimg.material.GetFloat("_Radius");
			delta = Mathf.MoveTowards(delta, 1f, recordingModeTrackerSO.UnscaledDeltaTime * popupEntrySpeed);
			po.UIimg.material.SetFloat("_Radius", delta);
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
				new Vector2(xPos, -popupHeight / 2f - popupHeight * (popupViewLimit - i - 1)),
				recordingModeTrackerSO.UnscaledDeltaTime * popupMoveSpeed);
			if (delta >= 0.833f)
			{
				ActivateUIDetails(po, true);
				po.name.color = po.description.color =
					Color.Lerp(Color.white, textColor, (delta - 0.833f) / textFadeSpeed);
			}

			po.AddTimer(recordingModeTrackerSO.UnscaledDeltaTime);
			if (po.timer >= fullDelay)
			{
				RemovePopup(i);
			}
		}

		for (int i = 0; i < inactivePopups.Count; i++)
		{
			ItemPopupObject po = inactivePopups[i];
			po.UIimg.material.SetFloat("_Flash", 0f);
			float delta = po.UIimg.material.GetFloat("_Radius");
			po.UIimg.material.SetFloat("_Radius",
				Mathf.MoveTowards(delta, 0f, recordingModeTrackerSO.UnscaledDeltaTime));
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
				new Vector2(xPos, po.transform.anchoredPosition.y),
				recordingModeTrackerSO.UnscaledDeltaTime * popupMoveSpeed);
		}
	}

	protected override void RemovePopup(int index)
	{
		ItemPopupObject po = activePopups[index];
		inactivePopups.Add(po);
		activePopups.Remove(po);
		ActivateUIDetails(po, false);
	}

	private void ActivateUIDetails(ItemPopupObject po, bool activate)
	{
		po.spr.gameObject.SetActive(activate);
		po.name.gameObject.SetActive(activate);
		po.description.gameObject.SetActive(activate);
	}

	public void GeneratePopup(Item.Type type, int amount = 1)
	{
		PopupData data = new PopupData(sprites, type, amount);
		foreach (ItemPopupObject po in activePopups)
		{
			if (po.type == type)
			{
				po.AddAmount(data.amount);
				return;
			}
		}
		foreach (PopupData pd in popupsToShow)
		{
			if (pd.type == type)
			{
				pd.AddAmount(amount);
				return;
			}
		}
		popupsToShow.Add(data);
	}

	private class ItemPopupObject : PopupObject
	{
		private ItemSprites sprites;
		public Item.Type type;
		public Image UIimg, spr;
		public Text name, description;
		public int amount;

		public ItemPopupObject(ItemSprites sprites, RectTransform transform, Image UIimg, Image spr, Text name, Text description, Item.Type type = Item.Type.Blank, int amount = 0)
		{
			this.sprites = sprites;
			this.transform = transform;
			this.type = type;
			this.UIimg = UIimg;
			this.UIimg.material = Instantiate(this.UIimg.material);
			this.spr = spr;
			this.name = name;
			this.description = description;
			timer = 0f;
			this.amount = amount;
		}

		public int AddAmount(int amount)
		{
			this.amount += amount;
			timer = 0f;
			UpdateName();
			return this.amount;
		}

		private void UpdateName()
		{
			if (amount > 1)
			{
				name.text = string.Format("{0} (x{1})", type.ToString(), amount);
			}
			else
			{
				name.text = type.ToString();
			}
		}

		public void UpdateData(Item.Type? type = null)
		{
			if (type != null)
			{
				this.type = (Item.Type)type;
			}
			if (sprites)
			{
				spr.sprite = sprites.GetItemSprite(this.type);
			}
			UpdateName();
			description.text = Item.ItemDescription(this.type);
		}
	}

	private class PopupData
	{

		public Sprite spr;
		public Item.Type type;
		public string name, description;
		public int amount;

		public PopupData(ItemSprites sprites, Item.Type type, int amount = 1)
		{
			this.type = type;
			if (sprites)
			{
				spr = sprites.GetItemSprite(type);
			}
			else
			{
				spr = null;
			}
			name = type.ToString();
			description = Item.ItemDescription(type);
			this.amount = amount;
		}

		public void AddAmount(int amount)
		{
			this.amount += amount;
		}
	}
}