using UnityEngine;

public class MainHatchPrompt : InteractablePromptTrigger
{
	[SerializeField] private ShipEntryPanel shipEntryUI;
	[SerializeField] private ConversationEvent interactBeforeRepairedShuttle,
		interactBeforeRechargedShip;

	private bool isLocked = false;

	protected override void OnInteracted()
	{
		base.OnInteracted();

		if (isLocked) return;

		Pause.InstantPause(true);
		shipEntryUI = shipEntryUI ?? FindObjectOfType<ShipEntryPanel>();
		shipEntryUI?.OpenPanel();
	}

	protected override void DialogueResponse()
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
	}

	public void Lock(bool lockDoor)
	{
		isLocked = lockDoor;
	}
}