using System;
using UnityEngine;

namespace DialogueSystem
{
	public class CutsceneController : MonoBehaviour
	{
		public bool pauseDuringCutscene;
		private bool dialogueInitialPauseValue;
		public ConversationWithActions conversation;

		public void StartConversation()
		{
			dialogueInitialPauseValue = ActiveDialogueController._instance.pause;
			ActiveDialogueController._instance.pause = pauseDuringCutscene;
			conversation.endEvent.AddListener(() => ActiveDialogueController._instance.pause = dialogueInitialPauseValue);
			ActiveDialogueController.StartConversation(conversation);
		}
	}
}
