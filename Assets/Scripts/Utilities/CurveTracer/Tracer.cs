using UnityEngine;

namespace CurveTracerSystem
{
	public class Tracer : MonoBehaviour
	{
		[SerializeField] private float delta;
		public bool useWorldSpace;

		public float GetDelta() => delta;

		public virtual float SetDelta(float value) => delta = value % 1f;

		public float IncrementDelta(float value) => SetDelta(GetDelta() + value);

		protected Vector3 SetInterpolatedPosition(CurveData cd)
			=> SetPosition(GetInterpolatedPosition(cd, GetDelta()));

		protected virtual Vector3 GetInterpolatedPosition(CurveData cd, float delta)
			=> Vector3.LerpUnclamped(cd.positionA, cd.positionB, cd.curve?.Evaluate(delta) ?? 0f);

		protected virtual Vector3 SetPosition(Vector3 pos)
		{
			if (useWorldSpace)
			{
				transform.position = pos;
			}
			else
			{
				transform.localPosition = pos;
			}
			return pos;
		}

		protected virtual void OnValidate()
		{
			SetInterpolatedPosition(new CurveData());
		}
	}

}