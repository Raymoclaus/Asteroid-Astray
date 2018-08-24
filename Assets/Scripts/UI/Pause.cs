using UnityEngine;

public class Pause : MonoBehaviour
{
	public static bool IsPaused { get { return Mathf.Approximately(Time.timeScale, Mathf.Epsilon); } }
	public static float timeSinceOpen = 0f;
	public static bool isShifting = false;
	private bool shiftingUp = false;

	private void Update()
	{
		timeSinceOpen += Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.P) && !isShifting)
		{
			isShifting = true;
			shiftingUp = IsPaused;
		}

		if (isShifting)
		{
			float scl = Time.timeScale;
			scl += shiftingUp ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
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
}