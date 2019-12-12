using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScreen : MonoBehaviour
{
	[SerializeField] private Image img;
	[SerializeField] private Color defaultColor = Color.black;
	[SerializeField] private Sprite defaultImage;
	[SerializeField] private AnimationCurve fadeInCurve, fadeOutCurve;

	private static FadeScreen instance;
	private static FadeScreen Instance
	{
		get
		{
			return instance != null ? instance : (instance = FindObjectOfType<FadeScreen>());
		}
	}

	private static CanvasGroup cGroup;
	private static CanvasGroup CGroup
	{
		get
		{
			return cGroup != null ? cGroup : (cGroup = Instance?.GetComponent<CanvasGroup>());
		}
	}

	private static readonly AnimationCurve defaultCurve
		= AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public static void FadeIn(float duration = 1f, AnimationCurve curve = null,
		System.Action finishedAction = null)
	{
		curve = curve ?? defaultCurve;
		Coroutines.TimedAction(duration, (float delta) =>
		{
			CGroup.alpha = 1f - delta;
		}, finishedAction);
	}

	public static void FadeOut(float duration = 1f, AnimationCurve curve = null,
		System.Action finishedAction = null)
	{
		curve = curve ?? defaultCurve;
		Coroutines.TimedAction(duration, (float delta) =>
		{
			CGroup.alpha = delta;
		}, finishedAction);
	}

	public void FadeIn(float duration)
	{
		Coroutines.TimedAction(duration,
			delta => CGroup.alpha = fadeInCurve.Evaluate(delta),
			null);
	}

	public void FadeOut(float duration)
	{
		Coroutines.TimedAction(duration,
			delta => CGroup.alpha = fadeOutCurve.Evaluate(delta),
			null);
	}

	public static void SetColour(Color col) => instance.img.color = col;

	public static void ResetColorToDefault() => SetColour(instance.defaultColor);

	public static void SetImage(Sprite spr) => instance.img.sprite = spr;

	public static void ResetImageToDefault() => SetImage(instance.defaultImage);
}
