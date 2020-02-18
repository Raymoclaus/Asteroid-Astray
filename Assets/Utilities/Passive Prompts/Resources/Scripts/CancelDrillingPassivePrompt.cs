using UnityEngine;

public class CancelDrillingPassivePrompt : PassivePromptController
{
	[SerializeField] private Shuttle mainChar;

	private void Update()
	{
		if (mainChar == null) return;

		SetActive(!Pause.IsStopped && mainChar.hasControl && mainChar.IsDrilling && !mainChar.CanLaunch);
	}
}
