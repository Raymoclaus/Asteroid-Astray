using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
	[SerializeField] private DialoguePopupUI dialogueUI, chatUI;
	private ConversationEvent currentConversation;
	private DialogueLineEvent[] currentLines;
	private EntityProfile[] speakers;
	private int currentPosition = -1;
	private bool dialogueIsRunning, chatIsRunning;
	[SerializeField] private ConversationEvent testDialogue;
	private List<ConversationEvent> chatQueue = new List<ConversationEvent>();
	private float chatQueueTimer = 0f;
	[SerializeField] private float chatQueueWaitDuration = 2f;
	private float chatContinueTimer = 0f;
	[SerializeField] private float chatContinueWaitDuration = 4f;
	[SerializeField] private List<MoveTrigger> moveTriggers;

	private void Awake()
	{
		dialogueUI = dialogueUI ?? FindObjectOfType<DialoguePopupUI>();
		chatUI = chatUI ?? FindObjectOfType<CommPopupUI>();
	}

	private void Update()
	{
		if (testDialogue != null)
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				StartDialogue(testDialogue);
			}
			if (Input.GetKeyDown(KeyCode.Y))
			{
				StartChat(testDialogue);
			}
		}

		if (dialogueIsRunning && InputHandler.GetInputDown(InputAction.ScrollDialogue) > 0f)
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
					ConversationEvent nextChat = chatQueue[0];
					StartChat(nextChat);
					chatQueue.RemoveAt(0);
				}
			}
		}

		if (chatIsRunning)
		{
			if (InputHandler.GetInputDown(InputAction.ScrollDialogue) > 0f)
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
				currentLines = currentConversation.conversation;
				speakers = currentConversation.speakers;
				currentPosition = cep.position;
			}
			else
			{
				currentConversation.conversationEndAction?.Invoke();
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
		if (currentPosition < 0) return;

		int speakerID = currentLines[currentPosition].speakerID;
		string name = speakers[speakerID].entityName;
		string line = currentLines[currentPosition].line;
		Sprite face = speakers[speakerID].face;

		if (currentConversation.conversation[currentPosition].hasAction)
		{
			currentConversation.conversation[currentPosition].action.Invoke();
		}

		DialoguePopupUI popupUI = dialogueIsRunning ? dialogueUI : chatUI;
		popupUI.GeneratePopup(name, line, face, speakerID);
		popupUI.Type();
	}

	public void SkipEntireDialogue()
	{
		if (!dialogueIsRunning) return;

		for (; currentPosition < currentLines.Length; currentPosition++)
		{
			if (currentConversation.conversation[currentPosition].hasAction)
			{
				currentConversation.conversation[currentPosition].skipAction?.Invoke();
			}
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
			if (currentConversation.conversation[currentPosition].hasAction)
			{
				currentConversation.conversation[currentPosition].skipAction?.Invoke();
			}
		}

		GetNextLine();
	}

	public void StartDialogue(ConversationEvent newDialogue)
	{
		if (dialogueIsRunning || !newDialogue) return;

		dialogueIsRunning = true;
		Setup(newDialogue);
		Pause.InstantPause(true);
		MoveTriggerObjects(false);
	}

	public void StartChat(ConversationEvent newDialogue)
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

	private void Setup(ConversationEvent dialogue)
	{
		currentConversation = dialogue;
		currentLines = currentConversation.conversation;
		speakers = currentConversation.speakers;
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
}
