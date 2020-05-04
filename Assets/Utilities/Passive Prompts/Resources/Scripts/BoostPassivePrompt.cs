public class BoostPassivePrompt : PassivePromptController
{
	private Character MainCharacter => NarrativeManager.MainCharacter;

	private IPlayableCharacter PlayableCharacter => (IPlayableCharacter)MainCharacter;

	private void Update()
	{
		if (PlayableCharacter == null) return;

		SetActive(!TimeController.IsStopped && PlayableCharacter.HasControl && PlayableCharacter.CanBoost && PlayableCharacter.BoostPercentage > 0f);
	}
}
