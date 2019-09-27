using System.Collections;
using UnityEngine;
using InputHandler;

public class Pause : MonoBehaviour
{
	private static Pause instance;

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
	private PauseUIController PauseUI
	{
		get
		{
			return pauseUI ?? (pauseUI = FindObjectOfType<PauseUIController>());
		}
	}
	[SerializeField] private RecordingModeController recordingModeController;

	public delegate void PauseEventHandler(bool pausing);
	public static event PauseEventHandler OnPause;
	public static void ClearEvent()
	{
		OnPause = null;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		PauseUI.Activate(false, instant: true);
	}

	private void Update()
	{
		timeSinceOpen += Time.deltaTime;

		if (InputManager.GetInput("Pause") > 0f && !isShifting && canPause)
		{
			OnPause?.Invoke(!IsPaused);
			if (IsPaused)
			{
				PauseUI.Activate(false, () =>
				{
					isShifting = true;
					shiftingUp = IsPaused;
				});
			}
			else
			{
				isShifting = true;
				shiftingUp = IsPaused;
				PauseUI.Activate(true);
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

		Time.fixedDeltaTime = IsStopped ? 1f : 0.01666666f;
	}

	public static void InstantPause(bool pause)
	{
		isShifting = false;
		shiftingUp = false;
		Time.timeScale = pause ? 0f : intendedTimeSpeed;
		canPause = !pause;
	}

	public static void TemporaryPause(float time = 0.5f)
	{
		instance.StartCoroutine(TempPauseCoroutine(time));
	}

	private static IEnumerator TempPauseCoroutine(float time = 0.5f)
	{
		canPause = false;
		Time.timeScale = 0f;
		while (time > 0f)
		{
			time -= instance.recordingModeController.UnscaledDeltaTime;
			yield return null;
		}
		Time.timeScale = intendedTimeSpeed;
		canPause = true;
	}

	public static void BulletTime(bool activate, float timeSpeed = 0.1f)
	{
		if (activate)
		{
			instance.StartCoroutine(SlowDown(timeSpeed * intendedTimeSpeed));
		}
		else
		{
			slowDownEffect = false;
		}
	}

	public static void TemporarySlowDownEffect(float duration = 1f, float timeSpeed = 0.1f)
	{
		canPause = false;
		instance.StartCoroutine(SlowDown(timeSpeed * intendedTimeSpeed));
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

	public static void DelayedAction(System.Action a, float wait, bool useDeltaTime = false)
	{
		instance.StartCoroutine(Delay(a, wait, useDeltaTime));
	}

	private static IEnumerator Delay(System.Action a, float wait, bool useDeltaTime = false)
	{
		while (wait > 0f)
		{
			wait -= useDeltaTime ? Time.deltaTime : instance.recordingModeController.UnscaledDeltaTime;
			yield return null;
		}
		a();
	}
}