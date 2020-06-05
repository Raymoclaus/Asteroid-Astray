using InputHandlerSystem.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PromptSystem.UI
{
	public class PromptUI : MonoBehaviour
	{
		private RectTransform rect;
		private RectTransform Rect => rect ?? (rect = GetComponent<RectTransform>());
		[SerializeField] private InputIconTextMesh prompt;
		private string unformattedText;
		[SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		[SerializeField] private float popupAnimationTime = 0.5f, popupExaggeration = 2f;

		private void Awake()
		{
			PromptRequests.OnLatestPromptSelected += ReceiveLatestPrompt;
			PromptRequests.OnPromptUpdated += UpdatePrompt;
			prompt = GetComponentInChildren<InputIconTextMesh>();
			Rect.localScale = Vector3.zero;
		}

		private void OnDestroy()
		{
			PromptRequests.OnLatestPromptSelected -= ReceiveLatestPrompt;
		}

		private void ReceiveLatestPrompt(PromptRequestData requestData)
		{
			if (requestData.IsInvalid)
			{
				DeactivatePrompt(unformattedText);
			}
			else
			{
				string promptText = requestData.promptText;
				ActivatePrompt(promptText);
			}
		}

		private void UpdatePrompt(PromptRequestData requestData, string oldText)
		{
			if (oldText != unformattedText || requestData.IsInvalid) return;
			string promptText = requestData.promptText;
			ActivatePrompt(promptText);
		}

		private void ActivatePrompt(string text)
		{
			if (text == unformattedText) return;
			unformattedText = text;
			SetText(text);
			StopAllCoroutines();
			StartCoroutine(Popup(true));
		}

		private void DeactivatePrompt(string text)
		{
			if (text != unformattedText) return;
			unformattedText = null;
			StopAllCoroutines();
			StartCoroutine(Popup(false));
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
}
