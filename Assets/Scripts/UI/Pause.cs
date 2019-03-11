using System.Collections;
using UnityEngine;

public class Pause : MonoBehaviour
{
	private static bool isPaused = false;
	public static bool IsPaused { get { return isPaused && IsStopped; } }
	public static bool IsStopped { get { return Mathf.Approximately(Time.timeScale, 0f); } }
	public static float timeSinceOpen = 0f;
	public static bool isShifting = false;
	private static bool shiftingUp = false;
	private static bool slowDownEffect = false;
	private static bool canPause = true;
	public static float intendedTimeSpeed = 1f;
	[SerializeField] private PauseUIController pauseUI;
	[SerializeField] private RecordingModeController recordingModeController;

	private void Awake()
	{
		pauseUI = pauseUI ?? FindObjectOfType<PauseUIController>();
		if (pauseUI)
		{
			pauseUI.Activate(false, instant: true);
		}
	}

	private void Update()
	{
		timeSinceOpen += Time.deltaTime;

		if (InputHandler.GetInputDown(InputHandler.InputAction.Pause) > 0f && !isShifting && canPause)
		{
			if (IsPaused)
			{
				pauseUI = pauseUI ?? FindObjectOfType<PauseUIController>();
				if (pauseUI)
				{
					pauseUI.Activate(false, () =>
					{
						isShifting = true;
						shiftingUp = IsPaused;
					});
				}
				else
				{
					isShifting = true;
					shiftingUp = IsPaused;
				}
			}
			else
			{
				isShifting = true;
				shiftingUp = IsPaused;
				pauseUI = pauseUI ?? FindObjectOfType<PauseUIController>();
				if (pauseUI)
				{
					pauseUI.Activate(true);
				}
			}
		}

		if (isShifting)
		{
			float scl = Time.timeScale;
			scl += recordingModeController.UnscaledDeltaTime * (shiftingUp ? 2f : -2f);
			if (scl <= 0f || scl >= intendedTimeSpeed)
			{
				Time.timeScale = Mathf.Clamp(scl, 0f, intendedTimeSpeed);
				isPaused = !isPaused;
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
		canPause = !pause;
	}

	public void TemporaryPause(float time = 0.5f)
	{
		StartCoroutine(TempPauseCoroutine(time));
	}

	private IEnumerator TempPauseCoroutine(float time = 0.5f)
	{
		canPause = false;
		Time.timeScale = 0f;
		while (time > 0f)
		{
			time -= recordingModeController.UnscaledDeltaTime;
			yield return null;
		}
		Time.timeScale = intendedTimeSpeed;
		canPause = true;
	}

	public void BulletTime(bool activate, float timeSpeed = 0.1f)
	{
		if (activate)
		{
			StartCoroutine(SlowDown(timeSpeed * intendedTimeSpeed));
		}
		else
		{
			slowDownEffect = false;
		}
	}

	public void TemporarySlowDownEffect(float duration = 1f, float timeSpeed = 0.1f)
	{
		canPause = false;
		StartCoroutine(SlowDown(timeSpeed * intendedTimeSpeed));
		DelayedAction(() => { slowDownEffect = false; }, duration);
	}

	private IEnumerator SlowDown(float timeSpeed = 0.5f)
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

	public void DelayedAction(System.Action a, float wait, bool useDeltaTime = false)
	{
		StartCoroutine(Delay(a, wait, useDeltaTime));
	}

	private IEnumerator Delay(System.Action a, float wait, bool useDeltaTime = false)
	{
		while (wait > 0f)
		{
			wait -= useDeltaTime ? Time.deltaTime : recordingModeController.UnscaledDeltaTime;
			yield return null;
		}
		a();
	}
}