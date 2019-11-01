using System;
using System.Collections.Generic;
using TimerUtilities;
using UnityEngine;

namespace DialogueSystem
{
	public class DialogueController
	{
		public event Action OnRevealCharacter, OnLineRevealed; 

		private ConversationWithActions currentConversation;
		private DialogueEvent[] currentEvents;
		private EntityProfile[] speakers;
		private int currentPosition = -1;
		private List<ConversationWithActions> dialogueQueue = new List<ConversationWithActions>();
		private double characterRevealTime = 0.1f;
		private SmartTimer characterRevealTimer = new SmartTimer(),
			waitEventTimer = new SmartTimer();

		public string currentSpeakerName { get; private set; }
		public string currentSpeakerText { get; private set; }
		public int RevealedCharacterCount { get; private set; }
		public Sprite currentSpeakerFace { get; private set; }
		public int currentSpeakerID { get; private set; }
		public AudioClip currentSpeakerTone { get; private set; }

		public DialogueController(ConversationWithActions conversation)
		{
			StartDialogue(conversation);
		}

		private bool TypingDialogue { get; set; }
		private bool DialogueIsRunning => currentPosition >= 0 || dialogueQueue.Count > 0;

		public void Next()
		{
			if (TypingDialogue)
			{
				RevealAllCharacters();
			}
			else
			{
				GetNextEvent();
			}
		}

		public void Skip()
		{
			if (!DialogueIsRunning) return;

			for (; currentPosition < currentEvents.Length; currentPosition++)
			{
				currentConversation.InvokeEvent(currentPosition);
			}

			GetNextEvent();
		}

		private void GetNextEvent()
		{
			//increment to next event
			currentPosition++;
			if (currentPosition >= currentEvents.Length)
			{
				//look for a linked conversation
				ConversationEventPosition cep = currentConversation.GetNextConversation();
				if (cep != null)
				{
					//start at the appropriate place in the new conversation
					Setup(cep.conversation, cep.position);
				}
				else
				{
					//end all conversation
					currentConversation.InvokeEndEvent();
					currentPosition = -1;
					currentEvents = null;
					speakers = null;
				}
			}

			TriggerCurrentEvent();
		}

		private void TriggerCurrentEvent()
		{
			if (CurrentEvent == null) return;

			switch (CurrentEvent)
			{
				case DialogueTextEvent textEvent:
					ShowText(textEvent);
					break;
				case DialogueWaitEvent waitEvent:
					Wait(waitEvent);
					break;
				default:
					Debug.LogWarning($"No implementation for type: {CurrentEvent.GetType()}");
					return;
			}
		}

		private void Wait(DialogueWaitEvent waitEvent)
		{
			waitEventTimer.DelayedAction(waitEvent.waitDuration, GetNextEvent, false);
		}

		private void ShowText(DialogueTextEvent textEvent)
		{
			int speakerID = textEvent.speakerID;
			EntityProfile currentSpeaker = speakers[speakerID];
			currentSpeakerName = currentSpeaker.entityName;
			currentSpeakerText = textEvent.line;
			currentSpeakerFace = currentSpeaker.face;
			currentSpeakerTone = currentSpeaker.chatTone;
			OnLineRevealed?.Invoke();

			//characterRevealTimer.RepeatingAction(characterRevealTime, TmpTele);
			RevealAllCharacters();

			//TO REMOVE
			currentConversation.InvokeEvent(currentPosition);
		}

		private void RevealAllCharacters()
		{
			RevealedCharacterCount = int.MaxValue;
			OnRevealCharacter?.Invoke();
		}

		private DialogueEvent CurrentEvent
			=> currentEvents == null ||
			   currentPosition < 0
			   || currentPosition > currentEvents.Length
				? null : currentEvents[currentPosition];

		public void StartDialogue(ConversationWithActions newDialogue)
		{
			if (DialogueIsRunning || newDialogue == null) return;
			Setup(newDialogue);
			TriggerCurrentEvent();
		}

		private void Setup(ConversationWithActions dialogue, int position = 0)
		{
			currentConversation = dialogue;
			currentEvents = currentConversation.GetLines();
			speakers = currentConversation.GetSpeakers();
			currentPosition = position;
		}
	}
}