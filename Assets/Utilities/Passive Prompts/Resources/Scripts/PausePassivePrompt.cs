public class PausePassivePrompt : PassivePromptController
{
	private void Update()
	{
		SetActive(Pause.CanPause);
	}
}
