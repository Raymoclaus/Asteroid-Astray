using System.Collections.Generic;
using UnityEngine;
using InputHandler;

public class DialogueController : MonoBehaviour
{
	[SerializeField] private DialoguePopupUI dialogueUI, chatUI;
	private DialoguePopupUI DialogueUI
		=> dialogueUI ?? (dialogueUI = FindObjectOfType<DialoguePopupUI>());
	private DialoguePopupUI ChatUI
		=> chatUI ?? (chatUI = FindObjectOfType<DialoguePopupUI>());
	private ConversationWithActions currentConversation;
	private DialogueLineEvent[] currentLines;
	private EntityProfile[] speakers;
	private int currentPosition = -1;
	private bool dialogueIsRunning, chatIsRunning;
	private List<ConversationWithActions> chatQueue = new List<ConversationWithActions>();
	private float chatQueueTimer = 0f;
	[SerializeField] private float chatQueueWaitDuration = 2f;
	private float chatContinueTimer = 0f;
	[SerializeField] private float chatContinueWaitDuration = 4f;
	[SerializeField] private List<MoveTrigger> moveTriggers;

	private bool skipDialogue = false;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			skipDialogue = true;
		}

		if (dialogueIsRunning && (InputManager.GetInputDown("Scroll Dialogue") || skipDialogue))
		{
			if (dialogueUI.IsTyping())
			{
				dialogueUI.RevealAllCharacters();
			}
			else
			{
				GetNextLine();
			}
		}

		if (!dialogueIsRunning && !chatIsRunning)
		{
			chatQueueTimer += Time.deltaTime;
			if (chatQueueTimer >= chatQueueWaitDuration)
			{
				chatQueueTimer = 0f;
				if (chatQueue.Count > 0)
				{
					ConversationWithActions nextChat = chatQueue[0];
					StartChat(nextChat);
					chatQueue.RemoveAt(0);
				}
			}
		}

		if (chatIsRunning)
		{
			if (InputManager.GetInputDown("Scroll Dialogue") || skipDialogue)
			{
				if (chatUI.IsTyping())
				{
					chatUI.RevealAllCharacters();
				}
				else
				{
					GetNextLine();
				}
			}
			else
			{
				chatContinueTimer += Time.deltaTime;
				if (chatContinueTimer >= chatContinueWaitDuration)
				{
					chatContinueTimer = 0f;
					GetNextLine();
				}
			}
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
				currentLines = currentConversation.GetLines();
				speakers = currentConversation.GetSpeakers();
				currentPosition = cep.position;
			}
			else
			{
				currentConversation.InvokeEndEvent();
				currentPosition = -1;
				currentLines = null;
				speakers = null;
				if (dialogueIsRunning)
				{
					Pause.InstantPause(false);
					dialogueIsRunning = false;
					dialogueUI.RemoveAllPopups();
					MoveTriggerObjects(true);
				}
				if (chatIsRunning)
				{
					chatIsRunning = false;
					chatUI.RemoveAllPopups();
				}
			}
		}
		SendPopup();
	}

	private void SendPopup()
	{
		if (currentPosition < 0)
		{
			skipDialogue = false;
			return;
		}

		int speakerID = currentLines[currentPosition].speakerID;
		string name = speakers[speakerID].entityName;
		string line = currentLines[currentPosition].line;
		Sprite face = speakers[speakerID].face;
		AudioClip tone = speakers[speakerID].chatTone;

		currentConversation.InvokeEvent(currentPosition);

		DialoguePopupUI popupUI = dialogueIsRunning ? dialogueUI : chatUI;
		popupUI.GeneratePopup(name, line, face, speakerID, tone);
		popupUI.Type(new WaitForSecondsRealtime(0.03f), null, null);
	}

	public void SkipEntireDialogue()
	{
		if (!dialogueIsRunning) return;

		for (; currentPosition < currentLines.Length; currentPosition++)
		{
			currentConversation.InvokeEvent(currentPosition);
		}

		GetNextLine();
	}

	public void SkipEntireChat(bool clearQueue)
	{
		if (clearQueue)
		{
			chatQueue.Clear();
		}

		if (!chatIsRunning) return;

		for (; currentPosition < currentLines.Length; currentPosition++)
		{
			currentConversation.InvokeEvent(currentPosition);
		}

		GetNextLine();
	}

	public void StartDialogue(ConversationWithActions newDialogue, bool pause = true)
	{
		if (dialogueIsRunning || newDialogue == null) return;

		dialogueIsRunning = true;
		Setup(newDialogue);
		if (pause)
		{
			Pause.InstantPause(true);
		}
		MoveTriggerObjects(false);
	}

	public void StartChat(ConversationWithActions newDialogue)
	{
		if (dialogueIsRunning || chatIsRunning)
		{
			chatQueue.Add(newDialogue);
			return;
		}
		chatIsRunning = true;
		chatQueueTimer = 0f;
		Setup(newDialogue);
	}

	private void Setup(ConversationWithActions dialogue)
	{
		currentConversation = dialogue;
		currentLines = currentConversation.GetLines();
		speakers = currentConversation.GetSpeakers();
		currentPosition = 0;
		SendPopup();
	}

	private void MoveTriggerObjects(bool goToA)
	{
		for (int i = 0; i < moveTriggers.Count; i++)
		{
			moveTriggers[i].Move(goToA);
		}
	}

	public bool DialogueIsActive()
	{
		return dialogueIsRunning || chatIsRunning;
	}
}
