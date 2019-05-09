using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScreen : MonoBehaviour
{
	private static FadeScreen instance;
	private static FadeScreen Instance
	{
		get { return instance ?? (instance = FindObjectOfType<FadeScreen>()); }
	}

	private static CanvasGroup cGroup;
	private static CanvasGroup CGroup
	{
		get { return cGroup ?? (cGroup = Instance?.GetComponent<CanvasGroup>()); }
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
}
