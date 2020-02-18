using UnityEngine;

public abstract class PassivePromptController : MonoBehaviour
{
	[SerializeField] private PassivePrompt prompt;

	private void Awake()
	{
		prompt.SetActive(false);
	}

	/// <summary>
	/// Notifies the prompt to activate or deactivate depending on given value.
	/// </summary>
	public void SetActive(bool activate)
	{
		if (prompt == null) return;
		prompt.SetActive(activate);
	}
}
