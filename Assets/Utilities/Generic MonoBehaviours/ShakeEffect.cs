using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
	private float intensityGoal;
	private float intensityShift = 0.01f;
	private float intensity;

	private void Update()
	{
		//shift intensity towards the goal
		intensity = Mathf.MoveTowards(intensity,
			intensityGoal,
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
	}

	public void Begin(float intensityValue = 0f, float intensityGoalValue = 0f,
		float intensityShiftValue = 0.01f)
	{
		intensity = intensityValue;
		intensityShift = intensityShiftValue;
		enabled = true;
	}

	public void Stop()
	{
		intensity = 0f;
		SetPosition(Vector3.zero);
		enabled = false;
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