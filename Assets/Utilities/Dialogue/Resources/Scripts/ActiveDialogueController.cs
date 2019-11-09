using DialogueSystem.UI;
using UnityEngine;

namespace DialogueSystem
{
	public class ActiveDialogueController : GameDialogueController
	{
		public bool pause = true;

		protected override void Awake()
		{
			base.Awake();
			FindObjectOfType<ActiveDialoguePopupUI>()?.SetDialogueController(this);
		}
		
		public override void StartDialogue(ConversationWithActions newConversation, bool skip)
		{
			base.StartDialogue(newConversation, skip);
			if (pause)
			{
				Pause.InstantPause(true);
			}
		}

		protected override void EndDialogue()
		{
			base.EndDialogue();
			if (pause)
			{
				Pause.InstantPause(false);
			}
		}

		protected override float CharacterRevealSpeed => Time.unscaledDeltaTime;
	}
}