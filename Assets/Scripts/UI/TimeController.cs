using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
	private static TimeController instance;
	
	private static float intendedTimeSpeed = 1f;
	//different features put in a request to keep the game paused
	//game remains paused as long as list contains requests
	//float represents timescale so it allows you to slow down time instead of just pausing
	//the entry with the lowest float value is prioritised
	private static Dictionary<object, float> _timeSpeedRequests = new Dictionary<object, float>();

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Update()
	{
		//increment game timer
		TimeSinceOpen += Time.deltaTime;
		//update timescale with lowest request value
		Time.timeScale = GetLowestTimeScaleRequest();
		//adjust physics speed based on timescale
		Time.fixedDeltaTime = IsStopped ? 1f : (Time.timeScale / 60f);
	}

	private void SendTimeScaleRequest(object obj, float request)
	{
		bool requestMatchesIntendedSpeed = request == intendedTimeSpeed;
		if (!_timeSpeedRequests.ContainsKey(obj))
		{
			if (requestMatchesIntendedSpeed) return;
			_timeSpeedRequests.Add(obj, request);
			return;
		}

		if (requestMatchesIntendedSpeed)
		{
			RemoveTimeScaleRequest(obj);
			return;
		}

		_timeSpeedRequests[obj] = request;
	}

	private void RemoveTimeScaleRequest(object obj) => _timeSpeedRequests.Remove(obj);

	private float GetLowestTimeScaleRequest()
	{
		float lowestValue = intendedTimeSpeed;

		foreach (float request in _timeSpeedRequests.Values)
		{
			lowestValue = Mathf.Min(lowestValue, request);

			if (lowestValue <= 0f) return 0f;
		}

		return lowestValue;
	}

	public static bool IsStopped => Mathf.Approximately(Time.timeScale, 0f);

	public static float TimeSinceOpen { get; set; } = 0f;

	public static void SetTimeScale(object obj, float scale)
	{
		instance.SendTimeScaleRequest(obj, scale);
	}

	public static void RemoveRequest(object obj)
	{
		instance.RemoveTimeScaleRequest(obj);
	}

	public static void TemporarilySetTimeScale(object obj, float scale, float duration)
	{
		instance.StartCoroutine(TimeScaleCoroutine(obj, scale, duration));
	}

	private static IEnumerator TimeScaleCoroutine(object obj, float scale, float duration)
	{
		SetTimeScale(obj, scale);
		yield return new WaitForSecondsRealtime(duration);
		RemoveRequest(obj);
	}

	public static void DelayedAction(Action a, float wait, bool useDeltaTime = false)
	{
		instance.StartCoroutine(Delay(a, wait, useDeltaTime));
	}

	private static IEnumerator Delay(Action a, float wait, bool useDeltaTime = false)
	{
		while (wait > 0f)
		{
			wait -= useDeltaTime ? Time.deltaTime : Time.unscaledDeltaTime;
			yield return null;
		}
		a();
	}

	[SteamPunkConsoleCommand(command = "timescale", info = "Sets the speed of the game.")]
	public static void SetIntendedSpeed(float speed)
	{
		intendedTimeSpeed = speed;
	}
}