using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class ItemPopupUI : PopupUI
	{
		[SerializeField]
		protected float scrollDelay = 3f, fullDelay = 5f;
		protected float scrollDelayTimer = 0f;
		[SerializeField] private RectTransform popupPrefab;
		private List<PopupData> popupsToShow = new List<PopupData>();
		public Color textColor;
		[SerializeField]
		private float textFadeSpeed = 0.17f;
		private float xPos;
		[SerializeField]
		private ItemSprites sprites;

		protected override PopupObject GetAnInactivePopup
		{
			get
			{
				PopupObject po = base.GetAnInactivePopup;
				if (po == null)
				{
					po = CreatePopup();
				}

				return po;
			}
		}

		private ItemPopupObject CreatePopup()
		{
			RectTransform popup = Instantiate(popupPrefab, popupPrefab.position, Quaternion.identity, transform);
			ItemPopupObject po = new ItemPopupObject(sprites,
				popup,
				popup.GetComponent<Image>(),
				popup.GetChild(0).GetComponent<Image>(),
				popup.GetChild(1).GetChild(0).GetComponent<Text>(),
				popup.GetChild(1).GetChild(1).GetComponent<Text>());
			inactivePopups.Add(po);
			po.ActivateUIDetails(false);
			return po;
		}

		private float XPos => popupPrefab.rect.height;

		private void Update()
		{
			while (popupsToShow.Count > 0)
			{
				if (activePopups.Count == popupViewLimit)
				{
					scrollDelayTimer += Time.unscaledDeltaTime;
					if (scrollDelayTimer >= scrollDelay)
					{
						RemovePopupsWithID(activePopups.Count - 1);
						foreach (ItemPopupObject ipo in activePopups)
						{
							ipo.SetTimer(Mathf.Max(0f, ipo.Timer - scrollDelayTimer));
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
					ItemPopupObject po = (ItemPopupObject)GetAnInactivePopup;
					po.transform.anchoredPosition =
						new Vector2(xPos, -po.Height / 2f - popupViewLimit * (po.Height - 1));
					po.SetTimer(0f);
					po.amount = popupsToShow[0].amount;
					po.UpdateData(popupsToShow[0].type);
					popupsToShow.RemoveAt(0);
					po.UIimg.material.SetFloat("_Radius", 0f);
					po.transform.gameObject.SetActive(true);
				}
			}

			foreach (ItemPopupObject ipo in activePopups)
			{
				int ID = ipo.ID;
				ipo.UIimg.material.SetFloat("_Flash", 1f);
				float delta = ipo.UIimg.material.GetFloat("_Radius");
				float popupHeight = ipo.Height;
				float yPos = -popupHeight / 2f - popupHeight * (popupViewLimit - ID - 1);
				if (!Mathf.Approximately(delta, 1f) || !Mathf.Approximately(ipo.transform.anchoredPosition.y, yPos))
				{
					delta = Mathf.MoveTowards(delta, 1f, Time.unscaledDeltaTime * popupEntrySpeed);
					ipo.UIimg.material.SetFloat("_Radius", delta);
					ipo.transform.anchoredPosition = Vector2.Lerp(ipo.transform.anchoredPosition,
						new Vector2(xPos, yPos),
						Time.unscaledDeltaTime * popupMoveSpeed);
					if (delta >= 0.833f)
					{
						ipo.ActivateUIDetails(true);
						ipo.name.color = ipo.description.color =
							Color.Lerp(Color.white, textColor, (delta - 0.833f) / textFadeSpeed);
					}
				}

				ipo.AddTimer(Time.unscaledDeltaTime);
				if (ipo.Timer >= fullDelay)
				{
					RemovePopupsWithID(ID);
				}
			}

			foreach (ItemPopupObject ipo in inactivePopups)
			{
				ipo.UIimg.material.SetFloat("_Flash", 0f);
				float delta = ipo.UIimg.material.GetFloat("_Radius");
				if (!Mathf.Approximately(delta, 0f))
				{
					ipo.UIimg.material.SetFloat("_Radius",
						Mathf.MoveTowards(delta, 0f, Time.unscaledDeltaTime));
					ipo.transform.anchoredPosition = Vector2.Lerp(ipo.transform.anchoredPosition,
						new Vector2(xPos, ipo.transform.anchoredPosition.y),
						Time.unscaledDeltaTime * popupMoveSpeed);
					if (delta <= 0f)
					{
						ipo.transform.gameObject.SetActive(false);
					}
				}
			}
		}

		protected override void RemovePopup(PopupObject po)
		{
			ItemPopupObject ipo = (ItemPopupObject) po;
			ipo.ActivateUIDetails(false);
			base.RemovePopup(po);
		}

		public void GeneratePopup(Item.Type type, int amount = 1)
		{
			PopupData data = new PopupData(sprites, type, amount);
			foreach (ItemPopupObject ipo in activePopups)
			{
				if (ipo.type == type)
				{
					ipo.AddAmount(data.amount);
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
			=> GeneratePopup(stack.ItemType, stack.Amount);

		private class ItemPopupObject : PopupObject
		{
			private ItemSprites sprites;
			public Item.Type type;
			public Image UIimg, spr;
			public Text name, description;
			public int amount;

			public ItemPopupObject(ItemSprites sprites, RectTransform transform,
				Image UIimg, Image spr, Text name, Text description) : base(transform)
			{
				this.sprites = sprites;
				this.UIimg = UIimg;
				this.UIimg.material = Instantiate(this.UIimg.material);
				this.spr = spr;
				this.name = name;
				this.description = description;
			}

			public int AddAmount(int amount)
			{
				this.amount += amount;
				ResetTimer();
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
				description.text = Item.Description(this.type);
			}

			public void ActivateUIDetails(bool activate)
			{
				spr.enabled = activate;
				name.enabled = activate;
				description.enabled = activate;
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
				description = Item.Description(type);
				this.amount = amount;
			}

			public void AddAmount(int amount)
			{
				this.amount += amount;
			}
		}
	}
}