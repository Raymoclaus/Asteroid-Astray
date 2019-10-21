using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurveTracerSystem
{
	public class CurveTracer : Tracer
	{
		[SerializeField] private List<CurveData> curves = new List<CurveData>();
		[SerializeField] private int path;

		public override float SetDelta(float value)
		{
			while (value >= 1f)
			{
				value -= 1f;
				IncrementPath(1);
			}
			return base.SetDelta(value);
		}

		public int GetPath() => ValidatePath();

		public int SetPath(int value)
		{
			path = value;
			return ValidatePath();
		}

		private int ValidatePath() => path = curves.Count == 0 ? -1 : path % curves.Count;

		public int IncrementPath(int value) => SetPath(GetPath() + value);

		protected override Vector3 GetInterpolatedPosition(CurveData cd, float delta)
		{
			if (curves.Count == 0)
			{
				return transform.position;
			}
			else
			{
				return base.GetInterpolatedPosition(curves[GetPath()], GetDelta());
			}
		}

		protected CurveData GetCurveData() => curves.Count == 0 ? new CurveData() : curves[GetPath()];

		protected override void OnValidate() => SetInterpolatedPosition(GetCurveData());

		public void GoToEndOfPath(float speedMultiplier = 1f)
		{
			GoToEndOfPath(speedMultiplier, null, true, null);
		}

		public void GoToEndOfPath(float speedMultiplier = 1f, int? pathNum = null, bool resetDistance = true, System.Action reachedPathEndAction = null)
		{
			SetPath(pathNum ?? GetPath());
			SetDelta(resetDistance ? 0f : GetDelta());
			StartCoroutine(FollowPath(speedMultiplier, reachedPathEndAction));
		}

		private IEnumerator FollowPath(float speedMultiplier, System.Action reachedPathEndAction)
		{
			float timer = GetDelta();
			while (timer < 1f)
			{
				timer += Time.deltaTime * speedMultiplier;
				if (timer > 1f)
				{
					timer = 1f;
				}
				SetPosition(GetInterpolatedPosition(GetCurveData(), timer));
				SetDelta(timer);
				yield return null;
			}
			reachedPathEndAction?.Invoke();
		}
	}

}