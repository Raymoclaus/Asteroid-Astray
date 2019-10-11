using UnityEngine;
using PR = PromptSystem.PromptRequests;

public class PromptCreator : MonoBehaviour, ITriggerMessageReceiver
{
	public string text;
	[SerializeField] protected bool promptsEnabled = true;

	public bool PromptsEnabled
	{
		get => promptsEnabled;
		set => promptsEnabled = value;
	}

	public void EnteredTrigger(IActor actor)
	{
		if (actor.CanTriggerPrompts)
		{
			PR.PromptSendRequest(GenerateUniqueKey(text), text);
		}
	}

	public void ExitedTrigger(IActor actor)
	{
		if (actor.CanTriggerPrompts)
		{
			PR.PromptRemovalRequest(GenerateUniqueKey(text));
		}
	}

	public void AllExitedTrigger() { }

	private void OnDestroy()
	{
		PR.PromptRemovalRequest(GenerateUniqueKey(text));
	}

	public string GenerateUniqueKey(string text) => gameObject.GetInstanceID() + text;

	public bool CanReceiveMessagesFromLayer(int layer) => true;
}