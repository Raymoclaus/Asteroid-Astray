using System;
using DialogueSystem;

public interface IChatter
{
	event Action<ConversationWithActions, bool> OnSendActiveDialogue, OnSendPassiveDialogue;
}
