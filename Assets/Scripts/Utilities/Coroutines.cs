using System;
using System.Collections;
using UnityEngine;

public class Coroutines : MonoBehaviour
{
	private static Coroutines instance;
	public static Coroutines Instance
	{
		get { return instance ?? (instance = FindObjectOfType<Coroutines>()); }
	}

	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public static void TimedAction(float duration, Action<float> timedAction,
		Action finishedAction)
	{
		Instance.StartCoroutine(Instance.ActionTimer(duration, timedAction, finishedAction));
	}

	private IEnumerator ActionTimer(float duration, Action<float> timedAction,
		Action finishedAction)
	{
		yield return new ActionOverTime(duration, timedAction);
		finishedAction?.Invoke();
	}

}
