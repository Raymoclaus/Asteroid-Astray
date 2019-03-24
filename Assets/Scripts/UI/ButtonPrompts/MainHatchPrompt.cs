using UnityEngine;

public class MainHatchPrompt : InteractablePromptTrigger
{
	[SerializeField] private ShipEntryPanel shipEntryUI;

	private bool canBeAccessed = true;

	protected override void OnInteracted()
	{
		base.OnInteracted();

		if (!canBeAccessed) return;

		Pause.InstantPause(true);
		shipEntryUI = shipEntryUI ?? FindObjectOfType<ShipEntryPanel>();
		shipEntryUI?.OpenPanel();
	}

	public void Lock(bool lockDoor)
	{
		canBeAccessed = !lockDoor;
	}
}