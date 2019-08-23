using UnityEngine;

public class PromptTrigger : VicinityTrigger
{
	private static PromptUI prompts;
	private PromptUI Prompts => prompts ?? (prompts = FindObjectOfType<PromptUI>());
	[SerializeField] protected string text;
	[SerializeField] protected bool promptsEnabled = true;

	public void EnablePrompts(bool enable)
	{
		promptsEnabled = enable;
		if (enable) return;
		DeactivatePrompt();
	}

	protected override void EnterTrigger(Triggerer actor)
	{
		base.EnterTrigger(actor);
		if (!promptsEnabled || !actor.canTriggerPrompts) return;

		Prompts.ActivatePrompt(text);
	}

	protected override void ExitTrigger(Triggerer actor)
	{
		base.ExitTrigger(actor);
		if (!actor.canTriggerPrompts) return;

		DeactivatePrompt();
	}

	private void DeactivatePrompt() => Prompts.DeactivatePrompt(text);
}