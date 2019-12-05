using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
	public class DialogueController : MonoBehaviour
	{
		public event Action OnDialogueStarted, OnRevealCharacter,
			OnAllCharactersRevealed, OnLineRevealed, OnDialogueEnded,
			OnStartDelayedText; 

		private ConversationWithActions currentConversation;
		private CharacterProfile[] speakers;
		private int currentPosition = -1;
		private List<ConversationWithActions> dialogueQueue = new List<ConversationWithActions>();
		private const float CHARACTER_REVEAL_TIME = 0.03f;
		private float revealTimer;

		public string CurrentSpeakerName { get; private set; }
		public string CurrentSpeakerText { get; private set; }
		public int RevealedCharacterCount { get; private set; }
		public float CharacterRevealSpeedMultiplier { get; private set; } = 1f;
		public Sprite CurrentSpeakerFace { get; private set; }
		public int CurrentSpeakerID { get; private set; }
		public AudioClip CurrentSpeakerTone { get; private set; }

		[SerializeField] private AudioSource audioSource;

		protected virtual void Awake()
		{
			if (currentConversation == null)
			{
				enabled = false;
			}
		}

		protected virtual void Update()
		{
			if (!IsTyping || IsWaitingOnDelayedText) return;

			revealTimer += CharacterRevealSpeed * CharacterRevealSpeedMultiplier;
			if (revealTimer >= CHARACTER_REVEAL_TIME)
			{
				if (CurrentSpeakerTone != null)
				{
					//audioSource.Stop();
					//audioSource.Play();
					audioSource.PlayOneShot(CurrentSpeakerTone);
				}
			}
			while (revealTimer >= CHARACTER_REVEAL_TIME)
			{
				revealTimer -= CHARACTER_REVEAL_TIME;
				RevealedCharacterCount++;
				OnRevealCharacter?.Invoke();
			}

			if (!IsTyping)
			{
				OnAllCharactersRevealed?.Invoke();
			}
		}

		protected virtual float CharacterRevealSpeed => Time.deltaTime;

		protected bool IsTyping
			=> DialogueIsRunning
			&& RevealedCharacterCount < TextLength;

		protected int TextLength { get; set; }

		public bool DialogueIsRunning
			=> currentConversation != null
			   && (currentPosition >= 0 || dialogueQueue.Count > 0);

		protected virtual void Next()
		{
			if (!DialogueIsRunning || IsWaitingOnDelayedText) return;

			if (IsTyping)
			{
				RevealAllCharacters();
			}
			else
			{
				GetNextEvent();
			}
		}

		protected virtual void Skip()
		{
			if (!DialogueIsRunning) return;

			for (; currentPosition < currentConversation.Length; currentPosition++)
			{
				currentConversation.InvokeEvent(currentPosition);
			}

			GetNextEvent();
		}

		private void GetNextEvent()
		{
			if (!DialogueIsRunning) return;

			//increment to next event
			currentPosition++;
			if (currentPosition >= currentConversation.Length)
			{
				//look for a linked conversation
				ConversationEventPosition cep = currentConversation.NextConversation;
				if (cep != null)
				{
					//start at the appropriate place in the new conversation
					Setup(cep.conversation, cep.position);
				}
				else
				{
					//end all conversation
					EndDialogue();
				}
			}

			TriggerCurrentEvent();
		}

		protected virtual void EndDialogue()
		{
			if (!DialogueIsRunning) return;

			currentConversation.InvokeEndEvent();
			currentPosition = -1;
			speakers = null;
			enabled = false;
			OnDialogueEnded?.Invoke();
		}

		private void TriggerCurrentEvent()
		{
			if (CurrentLine == null) return;
			GetDialogueLine(CurrentLine);
		}

		private DialogueTextEvent CurrentLine
			=> currentConversation.GetLine(currentPosition);

		private void GetDialogueLine(DialogueTextEvent textEvent)
		{
			int speakerID = textEvent.speakerID;
			CharacterProfile currentSpeaker = speakers[speakerID];
			CurrentSpeakerID = speakerID;
			CurrentSpeakerName = currentSpeaker.characterName;
			CurrentSpeakerText = textEvent.line;
			TextLength = GetTextLength(CurrentSpeakerText);
			CurrentSpeakerFace = currentSpeaker.face;
			CurrentSpeakerTone = currentSpeaker.chatTone;
			audioSource.clip = CurrentSpeakerTone;
			CharacterRevealSpeedMultiplier = textEvent.characterRevealSpeedMultiplier;

			float delay = textEvent.delay;
			Action action = () =>
			{
				RevealedCharacterCount = 0;
				OnLineRevealed?.Invoke();
				currentConversation.InvokeEvent(currentPosition);
			};

			if (delay > 0f)
			{
				IsWaitingOnDelayedText = true;
				action += () => IsWaitingOnDelayedText = false;
				OnStartDelayedText?.Invoke();
				DelayedTextEvent(textEvent.delay, action);
			}
			else
			{
				action?.Invoke();
			}
		}

		protected virtual void DelayedTextEvent(float delay, Action action)
		{
			Coroutines.DelayedAction(delay, action);
		}

		public bool IsWaitingOnDelayedText { get; private set; }

		private void RevealAllCharacters()
		{
			RevealedCharacterCount = TextLength;
			OnRevealCharacter?.Invoke();
			OnAllCharactersRevealed?.Invoke();
		}

		protected virtual int GetTextLength(string text)
			=> text?.CountExcludingRichTextTags() ?? 0;

		public virtual void StartDialogue(ConversationWithActions newConversation, bool skip)
		{
			if ((DialogueIsRunning && !skip) || newConversation == null) return;
			enabled = true;
			Setup(newConversation);
			TriggerCurrentEvent();
			OnDialogueStarted?.Invoke();
		}

		private void Setup(ConversationWithActions conversation, int position = 0)
		{
			currentConversation = conversation;
			speakers = currentConversation.Speakers;
			currentPosition = position;
		}
	}
}