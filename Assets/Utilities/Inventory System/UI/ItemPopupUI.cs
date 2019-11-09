using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	public class ItemPopupUI : PopupUI
	{
		protected IInventoryHolder inventoryHolder;
		[SerializeField] protected float scrollDelay = 3f, fullDelay = 5f;
		protected float scrollDelayTimer = 0f;
		[SerializeField] private RectTransform popupPrefab;
		private List<PopupData> popupsToShow = new List<PopupData>();
		public Color textColor;
		[SerializeField] private float textFadeSpeed = 0.17f;
		[SerializeField] private ItemSprites sprites;

		public void SetInventoryHolder(IInventoryHolder newInventoryHolder)
		{
			if (newInventoryHolder == null
			    || newInventoryHolder == inventoryHolder) return;
			if (inventoryHolder != null)
			{
				inventoryHolder.OnItemCollected -= GeneratePopup;
			}

			inventoryHolder = newInventoryHolder;
			inventoryHolder.OnItemCollected += GeneratePopup;
		}

		private new ItemPopupObject GetAnInactivePopup
		{
			get
			{
				ItemPopupObject po = (ItemPopupObject)base.GetAnInactivePopup;
				if (po == null)
				{
					po = CreatePopup();
				}
				po.MaterialRadius = 0f;
				po.transform.gameObject.SetActive(true);
				//po.transform.anchoredPosition =
				//	new Vector2(XPos, -po.Height / 2f - popupViewLimit * (po.Height - 1));
				po.SetTimer(0f);
				ActivatePopup(po);
				return po;
			}
		}

		private ItemPopupObject CreatePopup()
		{
			RectTransform popup = Instantiate(popupPrefab, transform, false);
			Image UIImage = popup.GetComponent<Image>();
			Material materialCopy = Instantiate(UIImage.material);
			UIImage.material = materialCopy;
			ItemPopupObject po = new ItemPopupObject(popup,
				materialCopy,
				popup.GetChild(0).GetComponent<Image>(),
				popup.GetChild(1).GetChild(0).GetComponent<Text>(),
				popup.GetChild(1).GetChild(1).GetComponent<Text>());
			popup.anchoredPosition += Vector2.down * po.Height;
			inactivePopups.Add(po);
			po.ActivateElements(false);
			return po;
		}

		private float XPos => popupPrefab.anchoredPosition.x;

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
					PopupData data = popupsToShow[0];
					ItemPopupObject po = GetAnInactivePopup;
					po.Data = data;
					popupsToShow.RemoveAt(0);
				}
			}

			foreach (ItemPopupObject ipo in activePopups)
			{
				int ID = ipo.ID;
				ipo.MaterialFlash = 1f;
				float delta = ipo.MaterialRadius;
				float popupHeight = ipo.Height;
				float targetHeight = GetTargetHeight(ipo);
				if (!Mathf.Approximately(delta, 1f)
				    || !Mathf.Approximately(ipo.transform.anchoredPosition.y, targetHeight))
				{
					delta = Mathf.MoveTowards(delta, 1f, Time.unscaledDeltaTime * popupEntrySpeed);
					ipo.MaterialRadius = delta;
					ipo.transform.anchoredPosition = Vector2.Lerp(ipo.transform.anchoredPosition,
						new Vector2(XPos, targetHeight),
						Time.unscaledDeltaTime * popupMoveSpeed);
					if (delta >= 0.833f)
					{
						ipo.ActivateElements(true);
						Color colorDelta = Color.Lerp(
							Color.white,
							textColor,
							(delta - 0.833f) / textFadeSpeed);
						ipo.NameColour = ipo.DescriptionColour = colorDelta;
					}
				}

				ipo.AddTimer(Time.unscaledDeltaTime);
			}

			RemovePopupsWithTimerGreaterThanOrEqualToTime(fullDelay);

			foreach (ItemPopupObject ipo in inactivePopups)
			{
				ipo.MaterialFlash = 0f;
				float delta = ipo.MaterialRadius;
				if (!Mathf.Approximately(delta, 0f))
				{
					delta = Mathf.MoveTowards(delta, 0f, Time.unscaledDeltaTime * popupEntrySpeed);
					ipo.MaterialRadius = delta;
					Vector2 targetPos = new Vector2(
						XPos,
						ipo.transform.anchoredPosition.y);
					ipo.transform.anchoredPosition =
						Vector2.Lerp(ipo.transform.anchoredPosition,
						targetPos,
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
			ipo.ActivateElements(false);
			base.RemovePopup(po);
		}

		private void GeneratePopup(ItemStack stack)
			=> GeneratePopup(stack.ItemType, stack.Amount);

		private void GeneratePopup(Item.Type type, int amount = 1)
		{
			PopupData data = new PopupData(sprites, type, amount);
			foreach (ItemPopupObject ipo in activePopups)
			{
				if (ipo.Data.ItemType == type)
				{
					ipo.AddAmount(data.Amount);
					return;
				}
			}

			for (int i = 0; i < popupsToShow.Count; i++)
			{
				PopupData pd = popupsToShow[i];
				if (pd.ItemType == type)
				{
					pd.AddAmount(amount);
					return;
				}
			}

			if (ViewingLimitReached)
			{
				popupsToShow.Add(data);
			}
			else
			{
				ItemPopupObject po = GetAnInactivePopup;
				po.Data = data;
			}
		}

		private class ItemPopupObject : PopupObject
		{
			private const string materialRadiusPropertyName = "_Radius",
				materialFlashPropertyName = "_Flash";

			private PopupData data;
			private Material Material { get; set; }
			private Image Spr { get; set; }
			private Text Name { get; set; }
			private Text Description { get; set; }

			public ItemPopupObject(RectTransform transform, Material material, Image spr,
				Text name, Text description) : base(transform)
			{
				Material = material;
				Spr = spr;
				Name = name;
				Description = description;
			}

			public PopupData Data
			{
				get => data;
				set
				{
					if (value == null || value == data) return;
					if (Data != null)
					{
						data.OnItemTypeUpdated -= UpdateName;
						data.OnItemTypeUpdated -= UpdateDescription;
						data.OnItemTypeUpdated -= UpdateSprite;
						data.OnAmountUpdated -= UpdateName;
					}
					data = value;
					data.OnItemTypeUpdated += UpdateName;
					data.OnItemTypeUpdated += UpdateDescription;
					data.OnItemTypeUpdated += UpdateSprite;
					data.OnAmountUpdated += UpdateName;

					UpdateElements();
				}
			}

			public void AddAmount(int amount)
			{
				Data.AddAmount(amount);
				ResetTimer();
			}

			public float MaterialRadius
			{
				get => Material.GetFloat(materialRadiusPropertyName);
				set => Material.SetFloat(materialRadiusPropertyName, value);
			}

			public float MaterialFlash
			{
				get => Material.GetFloat(materialFlashPropertyName);
				set => Material.SetFloat(materialFlashPropertyName, value);
			}

			public Color NameColour
			{
				get => Name.color;
				set => Name.color = value;
			}

			public Color DescriptionColour
			{
				get => Description.color;
				set => Description.color = value;
			}

			private void UpdateElements()
			{
				UpdateSprite();
				UpdateName();
				UpdateDescription();
			}

			public void ActivateElements(bool activate)
			{
				Spr.enabled = activate;
				Name.enabled = activate;
				Description.enabled = activate;
			}

			private void UpdateSprite()
			{
				Spr.sprite = Data.Spr;
			}

			private void UpdateName()
			{
				if (Data.Amount > 1)
				{
					Name.text = $"{Data.ItemName} {Data.Counter}";
					return;
				}
				Name.text = Data.ItemName;
			}

			private void UpdateDescription()
			{
				Description.text = Data.Description;
			}
		}

		private class PopupData
		{
			public event Action OnItemTypeUpdated, OnAmountUpdated;
			private ItemSprites Sprites { get; set; }
			public Item.Type ItemType { get; private set; }
			public int Amount { get; private set; }

			public PopupData(ItemSprites sprites, Item.Type itemType, int amount)
			{
				Sprites = sprites;
				SetItemType(itemType);
				SetAmount(amount);
			}

			public void SetItemType(Item.Type type)
			{
				ItemType = type;
				OnItemTypeUpdated?.Invoke();
			}

			public void SetAmount(int amount)
			{
				Amount = amount;
				OnAmountUpdated?.Invoke();
			}

			public void AddAmount(int amount)
			{
				SetAmount(Amount + amount);
			}

			public Sprite Spr => Sprites?.GetItemSprite(ItemType);

			public string ItemName => Item.TypeName(ItemType);

			public string Description => Item.Description(ItemType);

			public string Counter => $"(x{Amount})";
		}
	}
}