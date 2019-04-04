using System.Collections;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
	private Coroutine coro;
	private float intensityGoal;
	private float intensityShift = 0.01f;
	private float intensity;
	private const float INTENSITY_LIMIT = 0.05f;

	private IEnumerator Shake()
	{
		while (true)
		{
			if (Pause.IsStopped) yield return null;

			//shift intensity towards the goal but don't surpass the limit
			intensity = Mathf.MoveTowards(intensity,
				intensityGoal > INTENSITY_LIMIT ? INTENSITY_LIMIT : intensityGoal,
				intensityShift * Time.deltaTime * 60f);
			if (intensity == 0f)
			{
				Stop();
			}

			//set random position
			Vector3 pos = new Vector2(
				Mathf.Sin(Random.value * 2f * Mathf.PI),
				Mathf.Cos(Random.value * 2f * Mathf.PI))
				* intensity;
			SetPosition(pos);
			//wait for next frame
			yield return null;
		}
	}

	public void Begin(float intensityValue = 0f, float intensityGoalValue = 0f,
		float intensityShiftValue = 0.01f)
	{
		Stop();
		coro = StartCoroutine(Shake());
		intensity = intensityValue;
		intensityShift = intensityShiftValue;
	}

	public void Stop()
	{
		intensity = 0f;
		SetPosition(Vector3.zero);
		if (coro == null) return;
		StopCoroutine(coro);
	}

	public void SetIntensity(float val)
	{
		intensityGoal = val;
	}

	protected virtual void SetPosition(Vector3 pos)
	{
		transform.localPosition = pos;
	}
}