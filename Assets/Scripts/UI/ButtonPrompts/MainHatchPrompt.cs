using System;
using DialogueSystem;
using InputHandlerSystem;
using StatisticsTracker;
using TriggerSystem;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver, IChatter
{
	[SerializeField] private BoolStatTracker shuttleRepairedStat, shipRechargedStat;
	[SerializeField] private ConversationWithActions
		interactBeforeRepairedShuttle,
		interactBeforeRechargedShip,
		genericCantEnterShipConversation;
	
	public bool IsLocked { get; set; }
	public bool CanSendDialogue { get; set; } = true;

	[SerializeField] private Animator anim;

	public event Action<ConversationWithActions, bool> OnSendActiveDialogue;
	public event Action<ConversationWithActions, bool> OnSendPassiveDialogue;

	public void PlayDialogueResponse()
	{
		if (shuttleRepairedStat.IsFalse)
		{
			OnSendPassiveDialogue?.Invoke(interactBeforeRepairedShuttle, false);
		}
		else if (shipRechargedStat.IsFalse)
		{
			OnSendPassiveDialogue?.Invoke(interactBeforeRechargedShip, false);
		}
		else
		{
			OnSendPassiveDialogue?.Invoke(genericCantEnterShipConversation, false);
		}
	}

	public void Open() => anim.SetTrigger("Open");

	public void Interacted(IInteractor interactor, GameAction action)
	{
		if (IsLocked)
		{
			PlayDialogueResponse();
		}
		else
		{
			Open();
			interactor.Interact(this);
		}
	}

	public void AllowSendingDialogue(bool allow)
	{
		CanSendDialogue = allow;
	}
}