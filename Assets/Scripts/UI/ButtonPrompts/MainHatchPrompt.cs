using UnityEngine;
using UnityEngine.Events;

public class MainHatchPrompt : InteractablePromptTrigger
{
	[SerializeField] private ConversationWithActions
		interactBeforeRepairedShuttle,
		interactBeforeRechargedShip,
		genericCantEnterShipDialogue;

	public UnityEvent lockedActions;
	private bool isLocked = false;

	[SerializeField] private Animator anim;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnInteracted()
	{
		if (isLocked) return;
		base.OnInteracted();
	}

	public void PlayDialogueResponse()
	{
		if (DialogueController.DialogueIsActive()) return;

		if (!NarrativeManager.ShuttleRepaired)
		{
			DialogueController.StartChat(interactBeforeRepairedShuttle);
		}
		else if (!NarrativeManager.ShipRecharged)
		{
			DialogueController.StartChat(interactBeforeRechargedShip);
		}
		else
		{
			DialogueController.StartChat(genericCantEnterShipDialogue);
		}
	}

	protected override void ActivateInteractionActions()
	{
		if (!isLocked)
		{
			base.ActivateInteractionActions();
		}
	}

	public void Lock(bool lockDoor) => isLocked = lockDoor;

	public void Open() => anim.SetTrigger("Open");
}