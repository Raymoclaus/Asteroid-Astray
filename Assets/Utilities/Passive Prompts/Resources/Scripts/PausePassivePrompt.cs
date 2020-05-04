using InputHandlerSystem;
using UnityEngine;

public class PausePassivePrompt : PassivePromptController
{
	[SerializeField] private GameAction _pauseAction;

	private void Update()
	{
		SetActive(InputManager.CurrentContextContainsAction(_pauseAction));
	}
}
