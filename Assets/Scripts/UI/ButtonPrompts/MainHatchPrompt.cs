using DialogueSystem;
using DialogueSystem.UI;
using TriggerSystem;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver
{
	[SerializeField] private ConversationWithActions
		interactBeforeRepairedShuttle,
		interactBeforeRechargedShip,
		genericCantEnterShipDialogue;
	
	public bool IsLocked { get; set; }

	[SerializeField] private Animator anim;

	public void PlayDialogueResponse()
	{
		if (!NarrativeManager.ShuttleRepaired)
		{
			CommPopupUI.ShowDialogue(new DialogueController(interactBeforeRepairedShuttle));
		}
		else if (!NarrativeManager.ShipRecharged)
		{
			CommPopupUI.ShowDialogue(new DialogueController(interactBeforeRechargedShip));
		}
		else
		{
			CommPopupUI.ShowDialogue(new DialogueController(genericCantEnterShipDialogue));
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