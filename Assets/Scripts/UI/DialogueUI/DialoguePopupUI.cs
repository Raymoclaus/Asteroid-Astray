using System.Collections.Generic;
using UnityEngine;

public class DialoguePopupUI : PopupUI
{
	[SerializeField]
	private RectTransform leftPopupPrefab, rightPopupPrefab;
	protected List<DialoguePopupObject> activePopups = new List<DialoguePopupObject>();
	protected List<DialoguePopupObject> inactivePopups = new List<DialoguePopupObject>();
	protected List<int> speakerIDs = new List<int>();

	public bool hasName = true, hasLine = true, hasFace = true;

	private void Awake()
	{
		int amountOfPopupsToCreate = (popupViewLimit + 1) * 2;
		for (int i = 0; i < (popupViewLimit + 1) * 2; i++)
		{
			RectTransform popup = Instantiate(i < amountOfPopupsToCreate / 2 ? leftPopupPrefab : rightPopupPrefab);
			popup.gameObject.SetActive(false);
			popup.SetParent(transform, false);
			DialoguePopupObject po = new DialoguePopupObject(
				popup,
				hasName ? popup.GetComponentInChildren<DialogueNameUI>() : null,
				hasLine ? popup.GetComponentInChildren<DialogueLineUI>() : null,
				hasFace ? popup.GetComponentInChildren<DialogueFaceUI>() : null,
				popup.GetComponent<CanvasGroup>(),
				i < amountOfPopupsToCreate / 2);
			inactivePopups.Add(po);
		}
		popupHeight = ((RectTransform)transform.GetChild(0)).rect.height;
	}

	protected virtual void Update()
	{
		if (loadingTrackerSO.isLoading) return;

		for (int i = 0; i < activePopups.Count; i++)
		{
			DialoguePopupObject po = activePopups[i];
			float targetHeight = popupHeight * i;
			float delta = 1f - Mathf.Abs(targetHeight - po.transform.anchoredPosition.y) / popupHeight;
			delta = Mathf.Lerp(delta, 1f, recordingModeTrackerSO.UnscaledDeltaTime * popupEntrySpeed);
			po.popupGroup.alpha = i == 0 ? delta : 1f / (i + 1);
			Vector2 targetPos = po.transform.anchoredPosition;
			targetPos.y = targetHeight;
			po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition, targetPos, delta);
		}

		for (int i = 0; i < inactivePopups.Count; i++)
		{
			DialoguePopupObject po = inactivePopups[i];
			float delta = po.popupGroup.alpha;
			if (delta <= 0.01f)
			{
				po.transform.gameObject.SetActive(false);
			}
			else
			{
				delta = Mathf.Lerp(delta, 0f, recordingModeTrackerSO.UnscaledDeltaTime);
				po.popupGroup.alpha = delta;
				Vector2 dir = po.isLeft ? Vector2.left : Vector2.right;
				po.transform.anchoredPosition += dir * popupMoveSpeed * recordingModeTrackerSO.UnscaledDeltaTime;
			}
		}
	}

	protected override void RemovePopup(int index)
	{
		DialoguePopupObject po = activePopups[index];
		inactivePopups.Add(po);
		activePopups.Remove(po);
	}

	public void RemoveAllPopups()
	{
		for (int i = activePopups.Count - 1; i >= 0; i--)
		{
			RemovePopup(i);
		}
	}

	public virtual void GeneratePopup(string name, string line, Sprite face, int speakerID)
	{
		bool useLeftSide = speakerID == 0;
		if (activePopups.Count == 0)
		{
			speakerIDs.Clear();
		}
		else if (activePopups.Count >= popupViewLimit)
		{
			RemovePopup(activePopups.Count - 1);
		}
		AddSpeakerID(speakerID);
		DialoguePopupObject po = GetInactivePopup(useLeftSide);
		activePopups.Insert(0, po);
		inactivePopups.Remove(po);
		po.SetAll(name, line, face, speakerID, 0f);
		Vector2 pos = po.transform.anchoredPosition;
		pos.x = po.xPos;
		pos.y = -popupHeight;
		po.transform.anchoredPosition = pos;
		po.transform.gameObject.SetActive(true);
	}

	protected void AddSpeakerID(int speakerID)
	{
		for (int i = 0; i < speakerIDs.Count; i++)
		{
			int id = speakerIDs[i];
			if (id == speakerID)
			{
				return;
			}
		}
		speakerIDs.Add(speakerID);
	}

	protected DialoguePopupObject GetInactivePopup(bool leftSidePopup)
	{
		for (int i = 0; i < inactivePopups.Count; i++)
		{
			DialoguePopupObject popup = inactivePopups[i];
			if (popup.isLeft == leftSidePopup)
			{
				return popup;
			}
		}
		return null;
	}

	protected class DialoguePopupObject : PopupObject
	{
		public DialogueNameUI name;
		public DialogueLineUI line;
		public DialogueFaceUI face;
		public CanvasGroup popupGroup;
		public int speakerID;
		public bool isLeft;
		public float xPos;

		public DialoguePopupObject(RectTransform transform, DialogueNameUI name, DialogueLineUI line, DialogueFaceUI face,
			CanvasGroup popupGroup, bool isLeft)
		{
			this.transform = transform;
			this.name = name;
			this.line = line;
			this.face = face;
			this.popupGroup = popupGroup;
			speakerID = 0;
			this.isLeft = isLeft;
			timer = 0f;
			xPos = transform.anchoredPosition.x;
		}

		public void SetName(string name)
		{
			if (this.name == null) return;

			this.name.nameText.text = name;
		}

		public void SetLine(string line)
		{
			if (this.line == null) return;

			this.line.lineText.text = line;
		}

		public void SetFace(Sprite face)
		{
			if (this.face == null) return;

			this.face.faceImage.sprite = face;
		}

		public void SetAlpha(float alpha)
		{
			popupGroup.alpha = alpha;
		}

		public void SetAll(string name, string line, Sprite face, int speakerID, float alpha)
		{
			SetName(name);
			SetLine(line);
			SetFace(face);
			this.speakerID = speakerID;
			SetAlpha(alpha);
		}
	}
}
