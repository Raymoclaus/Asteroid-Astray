using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
	[SerializeField]
	private DialoguePopupUI dialogueUI;
	private ConversationEvent currentConversation;
	private DialogueLineEvent[] currentLines;
	private EntityProfile[] speakers;
	private int currentPosition = -1;
	private bool dialogueIsRunning;

	private void Awake()
	{
		dialogueUI = dialogueUI ?? FindObjectOfType<DialoguePopupUI>();
	}

	private void Update()
	{
		if (dialogueIsRunning && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
		{
			GetNextLine();
		}
	}

	private void GetNextLine()
	{
		currentPosition++;
		if (currentPosition >= currentLines.Length)
		{
			ConversationEventPosition cep = currentConversation.GetNextConversation();
			if (cep != null)
			{
				currentConversation = cep.conversation;
				currentLines = currentConversation.conversation;
				speakers = currentConversation.speakers;
				currentPosition = cep.position;
			}
			else
			{
				currentPosition = -1;
				currentLines = null;
				speakers = null;
				dialogueIsRunning = false;
				dialogueUI.RemoveAllPopups();
				Pause.InstantPause(false);
			}
		}
		SendPopup();
	}

	private void SendPopup()
	{
		if (currentPosition < 0) return;

		int speakerID = currentLines[currentPosition].speakerID;
		string name = speakers[speakerID].entityName;
		string line = currentLines[currentPosition].GetLine();
		Sprite face = speakers[speakerID].face;
		dialogueUI.GeneratePopup(name, line, face, speakerID);
	}

	public void StartDialogue(ConversationEvent newDialogue)
	{
		dialogueIsRunning = true;
		currentConversation = newDialogue;
		currentLines = currentConversation.conversation;
		speakers = currentConversation.speakers;
		currentPosition = 0;
		Pause.InstantPause(true);
		SendPopup();
	}
}
