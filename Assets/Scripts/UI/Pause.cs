using System.Collections;
using UnityEngine;

public class Pause : MonoBehaviour
{
	public static bool IsPaused { get { return Mathf.Approximately(Time.timeScale, Mathf.Epsilon); } }
	public static float timeSinceOpen = 0f;
	public static bool isShifting = false;
	private static bool shiftingUp = false;

	private void Update()
	{
		timeSinceOpen += Time.deltaTime;

		if (InputHandler.GetInputDown("Pause") > 0f && !isShifting)
		{
			isShifting = true;
			shiftingUp = IsPaused;
		}

		if (isShifting)
		{
			float scl = Time.timeScale;
			scl += shiftingUp ? Time.unscaledDeltaTime * 2f : -Time.unscaledDeltaTime * 2f;
			if (scl <= 0f || scl >= 1f)
			{
				Time.timeScale = Mathf.Clamp01(scl);
				isShifting = false;
			}
			else
			{
				Time.timeScale = scl;
			}
		}
	}

	public static void InstantPause(bool pause)
	{
		isShifting = false;
		shiftingUp = false;
		Time.timeScale = pause ? 0f : 1f;
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
			time -= Time.unscaledDeltaTime;
			//time -= 1f / 60f;
			yield return null;
		}
		Time.timeScale = 1f;
	}
}