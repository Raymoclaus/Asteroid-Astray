using System;
using DialogueSystem;
using InputHandlerSystem;
using QuestSystem;
using StatisticsTracker;
using TriggerSystem;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver, IChatter, IWaypointable, IInteractable
{
	[SerializeField] private ConversationWithActions
		interactBeforeRepairedShuttle,
		interactBeforeRechargedShip,
		genericCantEnterShipConversation;
	
	public bool IsLocked { get; set; }
	public bool CanSendDialogue { get; set; } = true;

	[SerializeField] private Animator anim;

	public event Action<ConversationWithActions, bool> OnSendActiveDialogue;
	public event Action<ConversationWithActions, bool> OnSendPassiveDialogue;
	public event Action<IInteractor> OnInteracted;

	public string UniqueID { get; set; }

	public Vector3 Position
	{
		get => transform.position;
		set => Debug.Log("The position of this object cannot be changed in this way.", gameObject);
	}

	public void PlayDialogueResponse()
	{
		BoolStatTracker shuttleRepairedStat = (BoolStatTracker)StatisticsIO.GetTracker("Shuttle Repaired");
		BoolStatTracker shipRechargedStat = (BoolStatTracker)StatisticsIO.GetTracker("Ship Recharged");
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

		OnInteracted?.Invoke(interactor);
	}

	public void AllowSendingDialogue(bool allow)
	{
		CanSendDialogue = allow;
	}
}