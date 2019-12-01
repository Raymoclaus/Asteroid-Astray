using System;
using DialogueSystem;

public interface IChatter
{
	event Action<ConversationWithActions, bool> OnSendActiveDialogue, OnSendPassiveDialogue;
	void AllowSendingDialogue(bool allow);
	bool CanSendDialogue { get; }
}
