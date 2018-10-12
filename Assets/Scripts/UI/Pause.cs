using System.Collections;
using UnityEngine;

public class Pause : MonoBehaviour
{
	public static bool IsPaused { get { return Mathf.Approximately(Time.timeScale, Mathf.Epsilon); } }
	public static float timeSinceOpen = 0f;
	public static bool isShifting = false;
	private static bool shiftingUp = false;
	private static bool slowDownEffect = false;
	private static bool canPause = true;
	public static float intendedTimeSpeed = 1f;

	private void Update()
	{
		timeSinceOpen += Time.deltaTime;

		if (InputHandler.GetInputDown("Pause") > 0f && !isShifting && canPause)
		{
			isShifting = true;
			shiftingUp = IsPaused;
		}

		if (isShifting)
		{
			float scl = Time.timeScale;
			if (GameController.RecordingMode)
			{
				scl += shiftingUp ? 2f / 60f : -2f / 60f;
			}
			else
			{
				scl += shiftingUp ? Time.unscaledDeltaTime * 2f : -Time.unscaledDeltaTime * 2f;
			}
			if (scl <= 0f || scl >= intendedTimeSpeed)
			{
				Time.timeScale = Mathf.Clamp01(scl);
				isShifting = false;
			}
			else
			{
				Time.timeScale = scl;
			}
		}
		Time.fixedDeltaTime = Time.timeScale <= 0.01f ? 1f : 0.01666666f * Time.timeScale;
	}

	public static void InstantPause(bool pause)
	{
		isShifting = false;
		shiftingUp = false;
		Time.timeScale = pause ? 0f : intendedTimeSpeed;
	}

	public static void TemporaryPause(float time = 0.5f)
	{
		GameController.singleton.StartCoroutine(TempPauseCoroutine(time));
	}

	private static IEnumerator TempPauseCoroutine(float time = 0.5f)
	{
		Time.timeScale = 0f;
		while (time > 0f)
		{
			time -= GameController.RecordingMode ? 1f / 60f : Time.unscaledDeltaTime;
			yield return null;
		}
		Time.timeScale = intendedTimeSpeed;
	}

	public static void BulletTime(bool activate, float timeSpeed = 0.1f)
	{
		if (activate)
		{
			GameController.singleton.StartCoroutine(SlowDown(timeSpeed * intendedTimeSpeed));
		}
		else
		{
			slowDownEffect = false;
		}
	}

	public static void TemporarySlowDownEffect(float duration = 1f, float timeSpeed = 0.1f)
	{
		canPause = false;
		GameController.singleton.StartCoroutine(SlowDown(timeSpeed * intendedTimeSpeed));
		DelayedAction(() => { slowDownEffect = false; }, duration);
	}

	private static IEnumerator SlowDown(float timeSpeed = 0.5f)
	{
		slowDownEffect = true;
		while (slowDownEffect)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, timeSpeed, 0.5f);
			yield return null;
		}

		while (Time.timeScale < intendedTimeSpeed - 0.05f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, intendedTimeSpeed, 0.5f);
			yield return null;
		}
		Time.timeScale = intendedTimeSpeed;
		canPause = true;
	}

	public static void DelayedAction(System.Action a, float wait)
	{
		GameController.singleton.StartCoroutine(Delay(a, wait));
	}

	private static IEnumerator Delay(System.Action a, float wait)
	{
		while (wait > 0f)
		{
			wait -= GameController.RecordingMode ? 1f / 60f : Time.unscaledDeltaTime;
			yield return null;
		}
		a();
	}
}