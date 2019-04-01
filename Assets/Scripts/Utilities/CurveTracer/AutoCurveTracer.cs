using UnityEngine;

public class AutoCurveTracer : CurveTracer
{
	public bool useUnscaledTime;
	public float speedMultiplier = 1f;

	private void Update()
	{
		if (!Application.isPlaying && !checkMovementInEditor) return;

		IncrementDelta(GetFrameIncrement());
		SetPosition(GetInterpolatedPosition());
	}

	protected virtual float GetFrameIncrement()
	{
		float increment = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		return increment * speedMultiplier;
	}
}
