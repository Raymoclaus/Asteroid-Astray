using System;
using System.Collections;
using UnityEngine;
using CustomYieldInstructions;

namespace Utilities
{
	public static class Coroutines
	{
		private static MonoBehaviour go;
		private static MonoBehaviour MonoObj => go != null ? go
			: (go = new GameObject("Coroutines Object").AddComponent<MonoBehaviour>());

		public static void TimedAction(float duration, Action<float> timedAction,
			Action finishedAction)
			=> Start(ActionTimer(new ActionOverTime(duration, timedAction), finishedAction));

		private static IEnumerator ActionTimer(ActionOverTime timerAction,
			Action finishedAction)
		{
			yield return timerAction;
			finishedAction?.Invoke();
		}

		public static void DelayedAction(float time, Action action)
			=> Start(DelayedAction(new WaitForSeconds(time), action));

		private static IEnumerator DelayedAction(WaitForSeconds wait, Action action)
		{
			yield return wait;
			action?.Invoke();
		}

		private static void Start(IEnumerator coroutineMethod)
			=> MonoObj.StartCoroutine(coroutineMethod);
	}

}