using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
	private static PromptUI promptUI;
	[SerializeField] protected string text;
	[SerializeField] protected float fadeInTime = 0f, fadeOutTime = 0f;
	private bool triggerActive = false;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsTriggerActive() || !collision.attachedRigidbody.GetComponent<Shuttle>()) return;

		promptUI = promptUI ?? FindObjectOfType<PromptUI>();
		triggerActive = true;
		EnterTrigger();
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!IsTriggerActive() || !collision.attachedRigidbody.GetComponent<Shuttle>()) return;

		promptUI = promptUI ?? FindObjectOfType<PromptUI>();
		triggerActive = false;
		ExitTrigger();
	}

	protected virtual void EnterTrigger()
	{
		promptUI?.ActivatePrompt(text, fadeInTime);
	}

	protected virtual void ExitTrigger()
	{
		promptUI?.DeactivatePrompt(text, fadeOutTime);
	}

	protected bool IsTriggerActive()
	{
		return triggerActive;
	}
}