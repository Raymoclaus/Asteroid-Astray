using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTracker : MonoBehaviour
{
	private static TimerTracker instance;
	private static TimerTracker Instance
	{
		get
		{
			return instance
				?? (instance = FindObjectOfType<TimerTracker>())
				?? (instance = new GameObject("Timer Tracker").AddComponent<TimerTracker>());
		}
	}

	private Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();

	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		foreach (string key in timers.Keys)
		{
			timers[key].Update();
		}
	}

	public static void AddTimer(string ID, float timer, Action finishAction,
		Action<float> timerAction)
		=> Instance.timers.Add(ID, new TimerData(timer, timerAction, finishAction));

	public static float GetTimer(string ID) => Instance.timers[ID]?.GetTimer() ?? 0f;

	public static void SetTimer(string ID, float time)
	{
		if (Instance.timers.ContainsKey(ID))
		{
			Instance.timers[ID]?.SetTimer(time);
		}
		else
		{
			AddTimer(ID, time, null, null);
		}
	}

	public static void SetTimerAction(string ID, Action<float> timerAction)
	{
		if (Instance.timers.ContainsKey(ID))
		{
			Instance.timers[ID]?.SetTimerAction(timerAction);
		}
		else
		{
			AddTimer(ID, 0f, null, timerAction);
		}
	}

	public static void SetTimerAction(string ID, Action finishAction)
	{
		if (Instance.timers.ContainsKey(ID))
		{
			Instance.timers[ID]?.SetFinishAction(finishAction);
		}
		else
		{
			AddTimer(ID, 0f, finishAction, null);
		}
	}

	public static void RemoveTimer(string ID) => Instance.timers.Remove(ID);

	private class TimerData
	{
		private float timer;
		private Action<float> timerAction;
		private Action finishAction;

		public TimerData(float timer, Action<float> timerAction, Action finishAction)
		{
			SetTimer(timer);
			SetTimerAction(timerAction);
			SetFinishAction(finishAction);
		}

		public void Update()
		{
			if (timer <= 0f) return;
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
			timerAction?.Invoke(timer);
			if (timer <= 0f)
			{
				finishAction?.Invoke();
			}
		}

		public float GetTimer() => timer;

		public void SetTimer(float time) => timer = time;

		public void SetTimerAction(Action<float> timerAction) => this.timerAction = timerAction;

		public void SetFinishAction(Action finishAction) => this.finishAction = finishAction;
	}
}
