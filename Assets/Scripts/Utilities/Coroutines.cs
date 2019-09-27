using System;
using System.Collections;
using UnityEngine;

public class Coroutines : MonoBehaviour
{
	private static Coroutines instance;
	public static Coroutines Instance
	{
		get
		{
			return instance
				?? (instance = FindObjectOfType<Coroutines>())
				?? (instance = new GameObject("Coroutine Manager").AddComponent<Coroutines>());
		}
	}

	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void OnDestroy() => instance = null;

	public static void TimedAction(float duration, Action<float> timedAction,
		Action finishedAction)
		=> Start(Instance.ActionTimer(new ActionOverTime(duration, timedAction), finishedAction));

	private IEnumerator ActionTimer(ActionOverTime timerAction,
		Action finishedAction)
	{
		yield return timerAction;
		finishedAction?.Invoke();
	}

	public static void DelayedAction(float time, Action action)
		=> Start(Instance.DelayedAction(new WaitForSeconds(time), action));

	private IEnumerator DelayedAction(WaitForSeconds wait, Action action)
	{
		yield return wait;
		action?.Invoke();
	}

	private static void Start(IEnumerator coroutineMethod)
		=> Instance.StartCoroutine(coroutineMethod);

}
