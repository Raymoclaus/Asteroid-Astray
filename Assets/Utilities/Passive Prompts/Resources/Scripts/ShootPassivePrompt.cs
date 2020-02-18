using UnityEngine;

public class ShootPassivePrompt : PassivePromptController
{
	[SerializeField] private Shuttle mainChar;

	private void Update()
	{
		if (mainChar == null) return;

		SetActive(!Pause.IsStopped && mainChar.hasControl && mainChar.CanShoot);
	}
}
