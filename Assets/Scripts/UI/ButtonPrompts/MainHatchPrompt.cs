using UnityEngine;

public class MainHatchPrompt : InteractablePromptTrigger
{
	private static DialogueController dlgCtrl;
	protected static DialogueController DlgCtrl
		=> dlgCtrl ?? (dlgCtrl = FindObjectOfType<DialogueController>());

	[SerializeField] private ConversationWithActions
		interactBeforeRepairedShuttle,
		interactBeforeRechargedShip,
		genericCantEnterShipDialogue;
	
	public bool IsLocked { get; set; }

	[SerializeField] private Animator anim;

	public override void Interact(Triggerer actor)
	{
		base.Interact(actor);
		Shuttle shuttle = actor.GetComponent<Shuttle>();
		if (shuttle == null) return;
		if (IsLocked)
		{
			PlayDialogueResponse();
		}
		else
		{
			Open();
			shuttle.EnterShip(transform);
		}
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

	public void Open() => anim.SetTrigger("Open");
}