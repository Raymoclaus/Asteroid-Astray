using DialogueSystem;
using DialogueSystem.UI;
using TriggerSystem;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver
{
	[SerializeField] private DialogueController passiveDialogue;
	[SerializeField] private ConversationWithActions
		interactBeforeRepairedShuttle,
		interactBeforeRechargedShip,
		genericCantEnterShipConversation;
	
	public bool IsLocked { get; set; }

	[SerializeField] private Animator anim;

	public void PlayDialogueResponse()
	{
		if (!NarrativeManager.ShuttleRepaired)
		{
			passiveDialogue.StartDialogue(interactBeforeRepairedShuttle);
		}
		else if (!NarrativeManager.ShipRecharged)
		{
			passiveDialogue.StartDialogue(interactBeforeRechargedShip);
		}
		else
		{
			passiveDialogue.StartDialogue(genericCantEnterShipConversation);
		}
	}

	public void Open() => anim.SetTrigger("Open");

	public void Interacted(IInteractor interactor, string action)
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
}