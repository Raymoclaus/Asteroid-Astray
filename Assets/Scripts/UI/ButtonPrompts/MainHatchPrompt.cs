using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IActionMessageReceiver
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