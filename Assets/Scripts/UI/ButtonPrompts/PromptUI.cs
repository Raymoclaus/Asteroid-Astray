using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PromptUI : MonoBehaviour
{
	private RectTransform rect;
	private RectTransform Rect { get { return rect ?? (rect = GetComponent<RectTransform>()); } }
	[SerializeField] private InputIconTextMesh prompt;
	[SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
	[SerializeField] private float popupAnimationTime = 0.5f, popupExaggeration = 2f;

	public delegate void PromptUpdatedEventHandler(string text, bool activating);
	public static event PromptUpdatedEventHandler OnPromptUpdated;
	public static void ClearEvent() => OnPromptUpdated = null;

	private void Awake()
	{
		prompt = GetComponentInChildren<InputIconTextMesh>();
		OnPromptUpdated = null;
		Rect.localScale = Vector3.zero;
	}

	public void ActivatePrompt(string text)
	{
		OnPromptUpdated?.Invoke(text, true);
		SetText(text);
		StartCoroutine(Popup(true));
	}

	public void ActivatePromptTimer(string text, float totalDuration = 5f)
	{
		ActivatePrompt(text);
		StartCoroutine(TimedPrompt(text, totalDuration));
	}

	public void DeactivatePrompt(string text)
	{
		if (text != prompt.GetText()) return;
		OnPromptUpdated?.Invoke(text, false);
		StopAllCoroutines();
		StartCoroutine(Popup(false));
	}

	private IEnumerator TimedPrompt(string text, float totalDuration)
	{
		float timer = 0f;
		while (timer < totalDuration)
		{
			timer += Time.deltaTime;
			yield return null;
		}

		if (prompt.GetText() == text)
		{
			StartCoroutine(Popup(false));
		}
	}

	private IEnumerator Popup(bool popIn)
	{
		float timer = 0f;
		while (timer < popupAnimationTime)
		{
			timer += Time.unscaledDeltaTime;
			float delta = timer / popupAnimationTime;
			float curveEvaluation = popupCurve.Evaluate(popIn ? delta : 1f - delta);
			Rect.localScale = Vector3.one * Mathf.Pow(curveEvaluation, popupExaggeration);
			yield return null;
		}
	}

	private void SetText(string text)
	{
		prompt.SetText(text);
		LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
	}
}
