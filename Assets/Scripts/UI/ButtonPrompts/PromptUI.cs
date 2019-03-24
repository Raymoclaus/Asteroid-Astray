using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class PromptUI : MonoBehaviour
{
	private RectTransform rectTransform;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private InputIconTextMesh prompt;

	private void Awake()
	{
		prompt = GetComponentInChildren<InputIconTextMesh>();
		canvasGroup.alpha = 0f;
	}

	public void ActivatePrompt(string text, float fadeInDuration = 0f)
	{
		SetText(text);
		StartCoroutine(Fade(true, fadeInDuration));
	}

	public void ActivatePromptTimer(string text, float fadeInDuration, float fadeOutDuration, float totalDuration = 5f)
	{
		ActivatePrompt(text, fadeInDuration);
		StartCoroutine(TimedPrompt(text, fadeInDuration, fadeOutDuration, totalDuration));
	}

	public void DeactivatePrompt(string text, float fadeOutDuration = 0f)
	{
		if (text != prompt.GetText()) return;
		StartCoroutine(Fade(false, fadeOutDuration));
	}

	private IEnumerator Fade(bool fadeIn, float fadeTime)
	{
		float timer = 0f;
		while (timer < fadeTime)
		{
			timer += Time.deltaTime;

			float delta = timer / fadeTime;
			canvasGroup.alpha = fadeIn ? delta : 1f - delta;

			yield return null;
		}
	}

	private IEnumerator TimedPrompt(string text, float fadeInTime, float fadeOutTime, float totalDuration)
	{
		float timer = 0f;
		while (timer < totalDuration)
		{
			timer += Time.deltaTime;
			yield return null;
		}

		if (prompt.GetText() == text)
		{
			StartCoroutine(Fade(false, fadeOutTime));
		}
	}

	private void SetText(string text)
	{
		prompt.SetText(text);
		rectTransform = rectTransform ?? GetComponent<RectTransform>();
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
	}
}
