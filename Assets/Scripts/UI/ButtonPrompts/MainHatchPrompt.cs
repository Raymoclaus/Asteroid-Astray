using UnityEngine;

public class MainHatchPrompt : InteractablePromptTrigger
{
	[SerializeField] private ShipEntryPanel shipEntryUI;

	protected override void OnInteracted()
	{
		base.OnInteracted();

		Pause.InstantPause(true);
		shipEntryUI = shipEntryUI ?? FindObjectOfType<ShipEntryPanel>();
		shipEntryUI?.OpenPanel();
	}
}