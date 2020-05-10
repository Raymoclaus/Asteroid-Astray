using System;
using UnityEngine;

namespace DialogueSystem.UI
{
	public class ActiveDialoguePopupUI : GameDialoguePopupUI, IElementHider
	{
		[SerializeField] private UIGroupHider groupHider;
		[SerializeField] private CanvasGroup _canvasGroup;

		public event Action<IElementHider> OnActivate;
		public event Action<IElementHider> OnDeactivate;

		protected override void Update()
		{
			foreach (DialoguePopupObject dpo in activePopups)
			{
				int id = dpo.ID;
				float popupHeight = dpo.Height;
				float targetHeight = GetTargetHeight(dpo);
				float delta = 1f - Mathf.Abs(targetHeight - dpo.transform.anchoredPosition.y) / popupHeight;
				delta = Mathf.Lerp(delta, 1f, Time.unscaledDeltaTime * popupEntrySpeed);
				dpo.popupGroup.alpha = id == 0 ? delta : 1f / (id + 1);
				dpo.inputPrompt.gameObject.SetActive(id == 0);
				Vector2 targetPos = dpo.transform.anchoredPosition;
				targetPos.y = targetHeight;
				dpo.transform.anchoredPosition = Vector2.Lerp(dpo.transform.anchoredPosition, targetPos, delta);
			}

			foreach (DialoguePopupObject dpo in inactivePopups)
			{
				float delta = dpo.popupGroup.alpha;
				if (delta <= 0.01f)
				{
					dpo.transform.gameObject.SetActive(false);
				}
				else
				{
					delta = Mathf.Lerp(delta, 0f, Time.unscaledDeltaTime);
					dpo.popupGroup.alpha = delta;
					Vector2 dir = dpo.isLeft ? Vector2.left : Vector2.right;
					dpo.transform.anchoredPosition += dir * popupMoveSpeed * Time.unscaledDeltaTime;
				}
			}
		}
		
		public UIGroupHider GroupHider => groupHider;

		public override void SetDialogueController(DialogueController newController)
		{
			if (newController == null) return;
			if (dialogueController != null)
			{
				dialogueController.OnDialogueStarted -= Activate;
				dialogueController.OnDialogueEnded -= Deactivate;
			}
			base.SetDialogueController(newController);
			dialogueController.OnDialogueStarted += Activate;
			dialogueController.OnDialogueEnded += Deactivate;
		}

		private void Activate()
		{
			OnActivate?.Invoke(this);

			Coroutines.TimedAction(0.5f,
				delta => _canvasGroup.alpha = delta,
				null,
				true);
		}

		private void Deactivate()
		{
			OnDeactivate?.Invoke(this);

			Coroutines.TimedAction(0.5f,
				delta => _canvasGroup.alpha = 1f - delta,
				null,
				true);
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

		private void GeneratePopup(string name, string line,
			Sprite face, int speakerID, AudioClip tone)
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
			pos.y = -po.Height;
			po.transform.anchoredPosition = pos;
			po.transform.gameObject.SetActive(true);
		}
	}
}
