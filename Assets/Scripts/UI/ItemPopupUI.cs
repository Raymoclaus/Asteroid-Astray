using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InventorySystem;
using InventorySystem.UI;

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
			ActivateUIDetails(po, false);
		}
		popupHeight = ((RectTransform)transform.GetChild(0)).rect.height;
		xPos = ((RectTransform)transform.GetChild(0)).anchoredPosition.x;
	}

	private void Update()
	{
		if (LoadingController.IsLoading) return;

		while (popupsToShow.Count > 0)
		{
			if (activePopups.Count == popupViewLimit)
			{
				scrollDelayTimer += Time.unscaledDeltaTime;
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
				po.transform.gameObject.SetActive(true);
			}
		}

		for (int i = 0; i < activePopups.Count; i++)
		{
			ItemPopupObject po = activePopups[i];
			po.UIimg.material.SetFloat("_Flash", 1f);
			float delta = po.UIimg.material.GetFloat("_Radius");
			float yPos = -popupHeight / 2f - popupHeight * (popupViewLimit - i - 1);
			if (!Mathf.Approximately(delta, 1f) || !Mathf.Approximately(po.transform.anchoredPosition.y, yPos))
			{
				delta = Mathf.MoveTowards(delta, 1f, Time.unscaledDeltaTime * popupEntrySpeed);
				po.UIimg.material.SetFloat("_Radius", delta);
				po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
					new Vector2(xPos, yPos),
					Time.unscaledDeltaTime * popupMoveSpeed);
				if (delta >= 0.833f)
				{
					ActivateUIDetails(po, true);
					po.name.color = po.description.color =
						Color.Lerp(Color.white, textColor, (delta - 0.833f) / textFadeSpeed);
				}
			}

			po.AddTimer(Time.unscaledDeltaTime);
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
			if (!Mathf.Approximately(delta, 0f))
			{
				po.UIimg.material.SetFloat("_Radius",
					Mathf.MoveTowards(delta, 0f, Time.unscaledDeltaTime));
				po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
					new Vector2(xPos, po.transform.anchoredPosition.y),
					Time.unscaledDeltaTime * popupMoveSpeed);
				if (delta <= 0f)
				{
					po.transform.gameObject.SetActive(false);
				}
			}
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
		po.spr.enabled = activate;
		po.name.enabled = activate;
		po.description.enabled = activate;
	}

	public void GeneratePopup(Item.Type type, int amount = 1)
	{
		PopupData data = new PopupData(sprites, type, amount);
		for (int i = 0; i < activePopups.Count; i++)
		{
			ItemPopupObject po = activePopups[i];
			if (po.type == type)
			{
				po.AddAmount(data.amount);
				return;
			}
		}

		for (int i = 0; i < popupsToShow.Count; i++)
		{
			PopupData pd = popupsToShow[i];
			if (pd.type == type)
			{
				pd.AddAmount(amount);
				return;
			}
		}
		popupsToShow.Add(data);
	}

	public void GeneratePopup(ItemStack stack)
		=> GeneratePopup(stack.GetItemType(), stack.GetAmount());

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
				name.text = Item.TypeName(type);
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
			spr = sprites ? sprites.GetItemSprite(type) : null;
			name = Item.TypeName(type);
			description = Item.ItemDescription(type);
			this.amount = amount;
		}

		public void AddAmount(int amount)
		{
			this.amount += amount;
		}
	}
}