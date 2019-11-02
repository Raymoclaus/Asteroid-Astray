using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DialogueSystem.UI
{
	public abstract class GameDialoguePopupUI : PopupUI
	{
		protected DialogueController dialogueController;
		[SerializeField] private RectTransform leftPopupPrefab, rightPopupPrefab;
		protected List<int> speakerIDs = new List<int>();

		public bool hasName = true, hasLine = true, hasFace = true;

		protected abstract void Update();

		public void SetDialogueController(DialogueController newController)
		{
			if (newController == null) return;
			if (dialogueController != null)
			{
				dialogueController.OnLineRevealed += CreatePopupOfCurrentLine;
				dialogueController.OnDialogueEnded += RemoveAllPopups;
				dialogueController.OnRevealCharacter += RevealCharacters;
				dialogueController.OnDialogueStarted += UpdateUI;
			}
			dialogueController = newController;
			dialogueController.OnLineRevealed += CreatePopupOfCurrentLine;
			dialogueController.OnDialogueEnded += RemoveAllPopups;
			dialogueController.OnRevealCharacter += RevealCharacters;
			dialogueController.OnDialogueStarted += UpdateUI;
			UpdateUI();
		}

		private void RevealCharacters()
		{
			DialoguePopupObject po = (DialoguePopupObject)GetActivePopupWithID(0);
			if (po == null) return;
			po.SetCharacterRevealedCount(dialogueController.RevealedCharacterCount);
		}

		protected void UpdateUI()
		{
			RemoveAllPopups();
			if (!dialogueController.DialogueIsRunning) return;

			CreatePopupOfCurrentLine();
		}

		protected abstract void CreatePopupOfCurrentLine();

		protected DialoguePopupObject GetInactivePopup(bool left)
		{
			DialoguePopupObject po = null;
			foreach (DialoguePopupObject dpo in inactivePopups)
			{
				if (dpo.isLeft == left)
				{
					po = dpo;
					break;
				}
			}

			if (po == null)
			{
				po = CreatePopup(left);
			}
			ActivatePopup(po);
			RevealCharacters();
			return po;
		}

		protected DialoguePopupObject CreatePopup(bool left)
		{
			RectTransform prefab = left ? leftPopupPrefab : rightPopupPrefab;
			RectTransform popup = Instantiate(prefab);
			popup.gameObject.SetActive(false);
			popup.SetParent(transform, false);
			DialoguePopupObject po = new DialoguePopupObject(
				popup,
				hasName ? popup.GetComponentInChildren<DialogueNameUI>() : null,
				hasLine ? popup.GetComponentInChildren<DialogueLineUI>() : null,
				hasFace ? popup.GetComponentInChildren<DialogueFaceUI>() : null,
				popup.GetComponentInChildren<DialogueInputPromptUI>(),
				popup.GetComponent<CanvasGroup>(),
				prefab == leftPopupPrefab);
			inactivePopups.Add(po);
			return po;
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

		protected class DialoguePopupObject : PopupObject
		{
			public DialogueNameUI name;
			public DialogueLineUI line;
			public DialogueFaceUI face;
			public DialogueInputPromptUI inputPrompt;
			public CanvasGroup popupGroup;
			public int speakerID;
			public bool isLeft;
			public float xPos;

			public DialoguePopupObject(RectTransform transform, DialogueNameUI name,
				DialogueLineUI line, DialogueFaceUI face,
				DialogueInputPromptUI inputPrompt, CanvasGroup popupGroup,
				bool isLeft) : base(transform)
			{
				this.name = name;
				this.line = line;
				this.face = face;
				this.inputPrompt = inputPrompt;
				this.popupGroup = popupGroup;
				speakerID = 0;
				this.isLeft = isLeft;
				xPos = transform.anchoredPosition.x;
			}

			public void SetName(string name)
			{
				if (this.name == null) return;

				this.name.SetText(name);
			}

			public void SetLine(string line)
			{
				if (this.line == null) return;

				this.line.SetText(line);
			}

			public void SetCharacterRevealedCount(int count)
			{
				TextMeshProUGUI mesh = line.textMesh;
				mesh.maxVisibleCharacters = count;
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
}