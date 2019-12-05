using System;
using UnityEngine;

namespace DialogueSystem
{
	[RequireComponent(typeof(ChatBehaviour))]
	public class CutsceneController : MonoBehaviour, IChatter
	{
		public bool pauseDuringCutscene;
		private bool dialogueInitialPauseValue;
		public ActiveDialogueController dialogueController;
		public ConversationWithActions conversation;

		public void StartConversation()
		{
			dialogueInitialPauseValue = dialogueController.pause;
			dialogueController.pause = pauseDuringCutscene;
			conversation.endEvent.AddListener(() => dialogueController.pause = dialogueInitialPauseValue);
			OnSendActiveDialogue?.Invoke(conversation, true);
		}

		public bool CanSendDialogue { get; private set; } = true;

		public event Action<ConversationWithActions, bool> OnSendActiveDialogue;
		public event Action<ConversationWithActions, bool> OnSendPassiveDialogue;

		public void AllowSendingDialogue(bool allow)
		{
			CanSendDialogue = allow;
		}
	}
}
