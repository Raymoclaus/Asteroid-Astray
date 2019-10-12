using UnityEngine;

namespace CurveTracerSystem
{
	public class AutoCurveTracer : CurveTracer
	{
		public bool useUnscaledTime;
		public float speedMultiplier = 1f;

		private void Update()
		{
			IncrementDelta(GetFrameIncrement());
			SetInterpolatedPosition(GetCurveData());
		}

		protected virtual float GetFrameIncrement()
		{
			float increment = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			return increment * speedMultiplier;
		}

		protected override void OnValidate()
		{
			Update();
		}
	}

}