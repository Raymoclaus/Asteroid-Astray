using UnityEngine;

public class Pause : MonoBehaviour
{
	public static bool IsPaused = false;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Time.timeScale = IsPaused ? 1f : 0f;
			IsPaused = !IsPaused;
		}
	}
}