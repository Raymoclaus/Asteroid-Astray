﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommPopupUI : DialoguePopupUI
{
	protected override void Update()
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
				po.transform.anchoredPosition += Vector2.right * popupMoveSpeed * recordingModeTrackerSO.UnscaledDeltaTime;
			}
		}
	}

	public override void GeneratePopup(string name, string line, Sprite face, int speakerID)
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
		pos.y = +popupHeight;
		po.transform.anchoredPosition = pos;
		po.transform.gameObject.SetActive(true);
	}
}