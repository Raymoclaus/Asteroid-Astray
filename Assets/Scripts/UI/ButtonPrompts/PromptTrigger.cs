using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
	private static PromptUI promptUI;
	[SerializeField] protected string text;
	[SerializeField] protected float fadeInTime = 0f, fadeOutTime = 0f;
	private bool triggerActive = false;
	protected bool disablePrompt = false;
	private PromptUI PromptUI { get { return promptUI ?? (promptUI = FindObjectOfType<PromptUI>()); } }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsTriggerActive() || !collision.attachedRigidbody.GetComponent<Shuttle>()) return;
		triggerActive = true;
		EnterTrigger();
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!IsTriggerActive() || !collision.attachedRigidbody.GetComponent<Shuttle>()) return;
		triggerActive = false;
		ExitTrigger();
	}

	public void DisablePrompt(bool disable)
	{
		disablePrompt = disable;
		if (!disable) return;
		DeactivatePrompt();
	}

	protected virtual void EnterTrigger() => PromptUI?.ActivatePrompt(text, fadeInTime);
	protected virtual void ExitTrigger() => DeactivatePrompt();
	protected bool IsTriggerActive() => triggerActive;
	private void DeactivatePrompt() => PromptUI?.DeactivatePrompt(text, fadeOutTime);
}