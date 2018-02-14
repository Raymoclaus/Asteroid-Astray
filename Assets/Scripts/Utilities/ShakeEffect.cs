using System.Collections;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
	private Coroutine coro;
	private float intensityGoal;
	private float intensityShift = 0.01f;
	private float intensity;

	private IEnumerator Shake()
	{
		while (true)
		{
			intensity = Mathf.MoveTowards(intensity, intensityGoal, intensityShift);
			transform.localPosition = new Vector2(
				Mathf.Sin(Random.value * 2f * Mathf.PI),
				Mathf.Cos(Random.value * 2f * Mathf.PI))
				* intensity;
			yield return null;
		}
	}

	public void Begin(float? intensityValue = null)
	{
		Stop();
		coro = StartCoroutine(Shake());
		intensity = intensityValue ?? 0f;
	}

	public void Stop()
	{
		intensity = 0f;
		if (coro == null) return;
		StopCoroutine(coro);
	}

	public void SetIntensity(float val)
	{
		intensityGoal = val;
	}
}