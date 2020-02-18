using UnityEngine;

public class BoostPassivePrompt : PassivePromptController
{
	[SerializeField] private Shuttle mainChar;

	private void Update()
	{
		if (mainChar == null) return;

		SetActive(!Pause.IsStopped && mainChar.hasControl && mainChar.CanBoost && mainChar.BoostPercentage > 0f);
	}
}
