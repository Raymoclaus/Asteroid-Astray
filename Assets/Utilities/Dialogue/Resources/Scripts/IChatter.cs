using System;
using DialogueSystem;
using UnityEngine;

public interface IChatter
{
	event Action<ConversationWithActions, bool> OnSendActiveDialogue, OnSendPassiveDialogue;
	void AllowSendingDialogue(bool allow);
	bool CanSendDialogue { get; }
}
