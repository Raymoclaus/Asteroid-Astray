using UnityEngine;

[ExecuteInEditMode]
public class CurveTracer : MonoBehaviour
{
	[SerializeField] protected bool checkMovementInEditor;
	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
	[SerializeField] private float delta;
	private static float Delta { get; set; }
	public Vector3 positionA, positionB;
	public bool useWorldSpace;

	protected virtual void IncrementDelta(float increment) => Delta += increment;

	protected virtual void SetDelta(float value) => Delta = value;

	public float GetDelta() => Delta;

	protected virtual Vector3 GetInterpolatedPosition()
		=> Vector3.LerpUnclamped(positionA, positionB, curve.Evaluate(Delta));

	protected virtual void SetPosition(Vector3 pos)
	{
		if (!Application.isPlaying && !checkMovementInEditor) return;

		if (useWorldSpace)
		{
			transform.position = pos;
		}
		else
		{
			transform.localPosition = pos;
		}
	}

	protected virtual void OnValidate()
	{
		if (!checkMovementInEditor) return;
		Delta = delta;
		SetPosition(GetInterpolatedPosition());
	}
}
