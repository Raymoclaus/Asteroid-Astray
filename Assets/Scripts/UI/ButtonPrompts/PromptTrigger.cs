using UnityEngine;

public class PromptTrigger : VicinityTrigger
{
	public string text;
	[SerializeField] protected bool promptsEnabled = true;

	public bool PromptsEnabled
	{
		get => promptsEnabled;
		set => promptsEnabled = value;
	}
}