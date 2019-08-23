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

	protected override void OnInteracted(Triggerer actor)
	{
		if (isLocked) return;
		base.OnInteracted(actor);
	}

	public void PlayDialogueResponse()
	{
		if (DlgCtrl.DialogueIsActive()) return;

		if (!NarrativeManager.ShuttleRepaired)
		{
			DlgCtrl.StartChat(interactBeforeRepairedShuttle);
		}
		else if (!NarrativeManager.ShipRecharged)
		{
			DlgCtrl.StartChat(interactBeforeRechargedShip);
		}
		else
		{
			DlgCtrl.StartChat(genericCantEnterShipDialogue);
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