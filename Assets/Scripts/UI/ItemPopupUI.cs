using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : MonoBehaviour
{
	private RectTransform[] popups;
	private float popupHeight, xPos;
	private static List<PopupData> popupsToShow = new List<PopupData>();
	[SerializeField]
	private float scrollDelay = 3f, fullDelay = 5f;
	private float scrollDelayTimer = 0f;
	[SerializeField]
	private int popupViewLimit = 4;
	private List<PopupObject> activePopups = new List<PopupObject>();
	private List<PopupObject> inactivePopups = new List<PopupObject>();
	public Color textColor;
	[SerializeField]
	private float popupEntrySpeed = 2f, popupMoveSpeed = 5f;

	private void Awake()
	{
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
				scrollDelayTimer += Time.unscaledDeltaTime;
				if (scrollDelayTimer >= scrollDelay)
				{
					RemovePopup(activePopups.Count - 1);
					for (int i = 0; i < activePopups.Count; i++)
					{
						PopupObject po = activePopups[i];
						po.timer = Mathf.Max(0f, po.timer - scrollDelayTimer);
						activePopups[i] = po;
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
				po.timer = 0f;
				PopupData data = popupsToShow[0];
				po.spr.sprite = data.spr;
				po.name.text = data.name;
				po.description.text = data.description;
				popupsToShow.RemoveAt(0);
				po.UIimg.material.SetFloat("_Radius", 0f);
				activePopups[0] = po;
			}
		}

		for (int i = 0; i < activePopups.Count; i++)
		{
			PopupObject po = activePopups[i];
			po.UIimg.material.SetFloat("_Flash", 1f);
			float delta = po.UIimg.material.GetFloat("_Radius");
			po.UIimg.material.SetFloat("_Radius", Mathf.MoveTowards(delta, 1f, Time.unscaledDeltaTime * popupEntrySpeed));
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
				new Vector2(xPos, -popupHeight / 2f - popupHeight * (popupViewLimit - i - 1)),
				Time.unscaledDeltaTime * popupMoveSpeed);
			if (delta >= 0.833f)
			{
				po.spr.gameObject.SetActive(true);
				po.name.gameObject.SetActive(true);
				po.description.gameObject.SetActive(true);
			}

			po.timer += Time.unscaledDeltaTime;
			activePopups[i] = po;
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
				Mathf.MoveTowards(delta, 0f, Time.unscaledDeltaTime));
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition,
				new Vector2(xPos, po.transform.anchoredPosition.y),
				Time.unscaledDeltaTime * popupMoveSpeed);
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			GeneratePopup(null, "Hi1", "what");
		}
	}

	private void RemovePopup(int index)
	{
		PopupObject po = activePopups[index];
		inactivePopups.Add(po);
		activePopups.Remove(po);
		po.spr.gameObject.SetActive(false);
		po.name.gameObject.SetActive(false);
		po.description.gameObject.SetActive(false);
	}

	public static void GeneratePopup(PopupData data)
	{
		popupsToShow.Add(data);
	}

	public static void GeneratePopup(Sprite spr, string name, string description)
	{
		GeneratePopup(new PopupData(spr, name, description));
	}

	private struct PopupObject
	{
		public RectTransform transform;
		public Image UIimg, spr;
		public Text name, description;
		public float timer;

		public PopupObject(RectTransform transform, Image UIimg, Image spr, Text name, Text description)
		{
			this.transform = transform;
			this.UIimg = UIimg;
			this.UIimg.material = Instantiate(this.UIimg.material);
			this.spr = spr;
			this.name = name;
			this.description = description;
			timer = 0f;
		}
	}
}

public struct PopupData
{
	public Sprite spr;
	public string name, description;

	public PopupData(Sprite spr, string name, string description)
	{
		this.spr = spr;
		this.name = name;
		this.description = description;
	}
}