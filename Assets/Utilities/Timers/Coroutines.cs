using System;
using System.Collections;
using UnityEngine;
using CustomYieldInstructions;

public static class Coroutines
{
	private static MonoEventHolder meh;
	public static MonoEventHolder MonoObj => meh != null ? meh
		: (meh = new GameObject("Coroutines Object").AddComponent<MonoEventHolder>());

	public static Coroutine TimedAction(float duration, Action<float> timedAction,
		Action finishedAction, bool unscaledTime)
		=> Start(PActionTimer(new ActionOverTime(duration, timedAction, unscaledTime), finishedAction));

	private static IEnumerator PActionTimer(ActionOverTime timerAction,
		Action finishedAction)
	{
		yield return timerAction;
		finishedAction?.Invoke();
	}

	public static Coroutine DelayedAction(float time, Action action)
	{
		if (time <= 0f)
		{
			action?.Invoke();
			return null;
		}
		return Start(PDelayedAction(new WaitForSeconds(time), action));
	}

	public static Coroutine DelayedAction(WaitForSeconds time, Action action)
		=> Start(PDelayedAction(time, action));

	private static IEnumerator PDelayedAction(WaitForSeconds wait, Action action)
	{
		yield return wait;
		action?.Invoke();
	}

	public static Coroutine UnscaledDelayedAction(float time, Action action)
	{
		if (time <= 0f)
		{
			action?.Invoke();
			return null;
		}
		return Start(PDelayedAction(new WaitForSecondsRealtime(time), action));
	}

	public static Coroutine UnscaledDelayedAction(WaitForSecondsRealtime time, Action action)
		=> Start(PDelayedAction(time, action));

	private static IEnumerator PDelayedAction(WaitForSecondsRealtime wait, Action action)
	{
		yield return wait;
		action?.Invoke();
	}

	private static Coroutine Start(IEnumerator coroutineMethod)
		=> MonoObj.StartCoroutine(coroutineMethod);
}