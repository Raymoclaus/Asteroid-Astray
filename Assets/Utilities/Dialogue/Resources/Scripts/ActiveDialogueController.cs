using DialogueSystem.UI;
using UnityEngine;

namespace DialogueSystem
{
	public class ActiveDialogueController : GameDialogueController
	{
		protected override void Awake()
		{
			base.Awake();
			FindObjectOfType<ActiveDialoguePopupUI>()?.SetDialogueController(this);
		}
		
		public override void StartDialogue(ConversationWithActions newConversation)
		{
			base.StartDialogue(newConversation);
			Pause.InstantPause(true);
		}

		protected override void EndDialogue()
		{
			base.EndDialogue();
			Pause.InstantPause(false);
		}

		protected override float CharacterRevealSpeed => Time.unscaledDeltaTime;
	}
}