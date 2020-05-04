public class ShootPassivePrompt : PassivePromptController
{
	private Character MainCharacter => NarrativeManager.MainCharacter;

	private IPlayableCharacter PlayableCharacter => (IPlayableCharacter) MainCharacter;

	private void Update()
	{
		if (PlayableCharacter == null) return;

		SetActive(!TimeController.IsStopped && PlayableCharacter.HasControl && PlayableCharacter.CanShoot);
	}
}
