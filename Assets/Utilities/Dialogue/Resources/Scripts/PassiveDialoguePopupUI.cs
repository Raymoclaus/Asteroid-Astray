using UnityEngine;

namespace DialogueSystem.UI
{
	public class PassiveDialoguePopupUI : GameDialoguePopupUI
	{
		protected override void Update()
		{
			foreach (PopupObject po in activePopups)
			{
				DialoguePopupObject dpo = (DialoguePopupObject)po;
				int i = po.ID;
				float popupHeight = dpo.Height;
				float targetHeight = GetTargetHeight(dpo);
				float delta = 1f - Mathf.Abs(targetHeight - dpo.transform.anchoredPosition.y) / popupHeight;
				delta = Mathf.Lerp(delta, 1f, Time.unscaledDeltaTime * popupEntrySpeed);
				dpo.popupGroup.alpha = i == 0 ? delta : 1f / (i + 1);
				Vector2 targetPos = po.transform.anchoredPosition;
				targetPos.y = targetHeight;
				po.transform.anchoredPosition = Vector2.Lerp(po.transform.anchoredPosition, targetPos, delta);
			}

			foreach (PopupObject po in inactivePopups)
			{
				DialoguePopupObject dpo = (DialoguePopupObject)po;
				float delta = dpo.popupGroup.alpha;
				if (delta <= 0.01f)
				{
					dpo.transform.gameObject.SetActive(false);
				}
				else
				{
					delta = Mathf.Lerp(delta, 0f, Time.unscaledDeltaTime);
					dpo.popupGroup.alpha = delta;
					dpo.transform.anchoredPosition += Vector2.right * popupMoveSpeed * Time.unscaledDeltaTime;
				}
			}
		}

		protected void GeneratePopup(string name, string line, Sprite face, int speakerID, AudioClip tone)
		{
			bool useLeftSide = speakerID == 0;
			if (activePopups.Count == 0)
			{
				speakerIDs.Clear();
			}
			else if (activePopups.Count >= popupViewLimit)
			{
				RemovePopupsWithID(activePopups.Count - 1);
			}
			AddSpeakerID(speakerID);
			DialoguePopupObject po = GetInactivePopup(useLeftSide);
			po.SetAll(name, line, face, speakerID, 0f);
			Vector2 pos = po.transform.anchoredPosition;
			pos.x = po.xPos;
			pos.y = po.Height;
			po.transform.anchoredPosition = pos;
			po.transform.gameObject.SetActive(true);
		}

		protected override void CreatePopupOfCurrentLine()
		{
			GeneratePopup(dialogueController);
		}

		private void GeneratePopup(DialogueController dc)
		{
			if (dc == null) return;

			GeneratePopup(dc.CurrentSpeakerName, dc.CurrentSpeakerText,
				dc.CurrentSpeakerFace, dc.CurrentSpeakerID,
				dc.CurrentSpeakerTone);
		}
	}
}
