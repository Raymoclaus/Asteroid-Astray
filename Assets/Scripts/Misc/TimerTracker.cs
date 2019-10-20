using System;
using System.Collections.Generic;
using UnityEngine;

public static class TimerTracker
{
	private static MonoEventHolder meh;
	private static MonoEventHolder MonoObj => meh != null ? meh
		: (meh = CreateMonoBehaviour());

	private static Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();

	private static MonoEventHolder CreateMonoBehaviour()
	{
		MonoEventHolder meh = new GameObject("Timer Tracker Object")
			.AddComponent<MonoEventHolder>();
		meh.OnUpdateEvent += Update;
		return meh;
	}

	private static void Update()
	{
		foreach (string key in timers.Keys)
		{
			timers[key].Update();
		}
	}

	public static void AddTimer(string ID, float timer, Action finishAction,
		Action<float> timerAction)
	{
		if (MonoObj == null) return;
		timers.Add(ID, new TimerData(timer, timerAction, finishAction));
	}

	public static float GetTimer(string ID) => timers[ID]?.GetTimer() ?? 0f;

	public static void SetTimer(string ID, float time)
	{
		if (timers.ContainsKey(ID))
		{
			timers[ID]?.SetTimer(time);
		}
		else
		{
			AddTimer(ID, time, null, null);
		}
	}

	public static void SetTimerAction(string ID, Action<float> timerAction)
	{
		if (timers.ContainsKey(ID))
		{
			timers[ID]?.SetTimerAction(timerAction);
		}
		else
		{
			AddTimer(ID, 0f, null, timerAction);
		}
	}

	public static void SetFinishAction(string ID, Action finishAction)
	{
		if (timers.ContainsKey(ID))
		{
			timers[ID]?.SetFinishAction(finishAction);
		}
		else
		{
			AddTimer(ID, 0f, finishAction, null);
		}
	}

	public static void RemoveTimer(string ID) => timers.Remove(ID);

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
