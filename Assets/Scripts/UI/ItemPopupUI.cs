using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : MonoBehaviour
{
	private static ItemPopupUI singleton;
	private RectTransform[] popups;
	private float popupHeight, xPos;
	private static List<PopupData> popupsToShow = new List<PopupData>();
	[SerializeField]
	private float scrollDelay = 3f, fullDelay = 5f;
	private float scrollDelayTimer = 0f;
	[SerializeField]
	private int popupViewLimit = 4;
	private static List<PopupObject> activePopups = new List<PopupObject>();
	private List<PopupObject> inactivePopups = new List<PopupObject>();
	public Color textColor;
	[SerializeField]
	private float popupEntrySpeed = 2f, popupMoveSpeed = 5f, textFadeSpeed = 0.17f;
	[SerializeField]
	private LoadedResources loadRes;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
		}
		else
		{
			Destroy(gameObject);
		}

		popups = new RectTransform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++)
		{
			popups[i] = (RectTransform)transform.GetChild(i);
			PopupObject po = new PopupObject(popups[i],
				popups[i].GetComponent<Image>(),
				popups[i].GetChild(0).GetComponent<Image>(),
				popups[i].GetChild(1).GetChild(0).GetComponent<Text>(),
				popups[i].GetChild(1).GetChild(1).GetComponent<Text>());
			inactivePopups.Add(po);
		}
		popupHeight = popups[0].rect.height;
		xPos = popups[0].anchoredPosition.x;
	}

	private void Update()
	{
		if (GameController.IsLoading) return;

		while (popupsToShow.Count > 0)
		{
			if (activePopups.Count == popupViewLimit)
			{
				scrollDelayTimer += Time.deltaTime;
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
				PopupObject po = inactivePopups[0];
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
			PopupObject po = activePopups[i];
			po.UIimg.material.SetFloat("_Flash", 1f);
			float delta = po.UIimg.material.GetFloat("_Radius");
			delta = Mathf.MoveTowards(delta, 1f, Time.deltaTime * popupEntrySpeed);
			po.UIimg.material.SetFloat("_Radius", delta);
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
				new Vector2(xPos, -popupHeight / 2f - popupHeight * (popupViewLimit - i - 1)),
				Time.deltaTime * popupMoveSpeed);
			if (delta >= 0.833f)
			{
				ActivateUIDetails(po, true);
				po.name.color = po.description.color =
					Color.Lerp(Color.white, textColor, (delta - 0.833f) / textFadeSpeed);
			}

			po.AddTimer(Time.deltaTime);
			if (po.timer >= fullDelay)
			{
				RemovePopup(i);
			}
		}

		for (int i = 0; i < inactivePopups.Count; i++)
		{
			PopupObject po = inactivePopups[i];
			po.UIimg.material.SetFloat("_Flash", 0f);
			float delta = po.UIimg.material.GetFloat("_Radius");
			po.UIimg.material.SetFloat("_Radius",
				Mathf.MoveTowards(delta, 0f, Time.deltaTime));
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
				new Vector2(xPos, po.transform.anchoredPosition.y),
				Time.deltaTime * popupMoveSpeed);
		}
	}

	private void RemovePopup(int index)
	{
		PopupObject po = activePopups[index];
		inactivePopups.Add(po);
		activePopups.Remove(po);
		ActivateUIDetails(po, false);
	}

	private void ActivateUIDetails(PopupObject po, bool activate)
	{
		po.spr.gameObject.SetActive(activate);
		po.name.gameObject.SetActive(activate);
		po.description.gameObject.SetActive(activate);
	}

	public static void GeneratePopup(Item.Type type, int amount = 1)
	{
		PopupData data = new PopupData(type, amount);
		foreach (PopupObject po in activePopups)
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

	private class PopupObject
	{
		public RectTransform transform;
		public Item.Type type;
		public Image UIimg, spr;
		public Text name, description;
		public float timer;
		public int amount;

		public PopupObject(RectTransform transform, Image UIimg, Image spr, Text name, Text description, Item.Type type = Item.Type.Blank, int amount = 0)
		{
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
			spr.sprite = singleton.loadRes.GetItemSprite(this.type);
			UpdateName();
			description.text = Item.ItemDescription(this.type);
		}

		public void SetTimer(float time)
		{
			timer = time;
		}

		public void AddTimer(float time)
		{
			timer += time;
		}
	}

	private struct PopupData
	{
		public Sprite spr;
		public Item.Type type;
		public string name, description;
		public int amount;

		public PopupData(Item.Type type, int amount = 1)
		{
			this.type = type;
			spr = singleton.loadRes.GetItemSprite(type);
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