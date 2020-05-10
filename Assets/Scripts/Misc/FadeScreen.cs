using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScreen : MonoBehaviour
{
	[SerializeField] private Image img;
	[SerializeField] private Color defaultColor = Color.black;
	[SerializeField] private Sprite defaultImage;
	[SerializeField] private AnimationCurve fadeInCurve, fadeOutCurve;

	private CanvasGroup cGroup;
	private CanvasGroup CGroup => cGroup != null
		? cGroup
		: (cGroup = GetComponent<CanvasGroup>());

	public void FadeIn(float duration = 1f, AnimationCurve curve = null,
		System.Action finishedAction = null,
		System.Action<float> updatedAction = null)
	{
		curve = curve ?? fadeInCurve;
		Coroutines.TimedAction(duration,
			delta =>
			{
				CGroup.alpha = EvaluateCurve(curve, delta);
				updatedAction?.Invoke(delta);
			},
			finishedAction,
			false);
	}

	public void FadeOut(float duration = 1f, AnimationCurve curve = null,
		System.Action finishedAction = null,
		System.Action<float> updatedAction = null)
	{
		curve = curve ?? fadeOutCurve;
		Coroutines.TimedAction(duration,
			delta =>
			{
				CGroup.alpha = EvaluateCurve(curve, delta);
				updatedAction?.Invoke(delta);
			},
			finishedAction,
			false);
	}

	public void FadeIn(float duration)
	{
		FadeIn(duration, fadeInCurve, null, null);
	}

	public void FadeOut(float duration)
	{
		FadeOut(duration, fadeOutCurve, null, null);
	}

	public void SetColour(Color col) => img.color = col;

	public void ResetColorToDefault() => SetColour(defaultColor);

	public void SetColourBlack() => SetColour(Color.black);

	public void SetColourWhite() => SetColour(Color.white);

	public void SetImage(Sprite spr) => img.sprite = spr;

	public void ResetImageToDefault() => SetImage(defaultImage);

	private float EvaluateCurve(AnimationCurve curve, float delta)
		=> curve.Evaluate(delta);
}
