using UnityEngine;
using System.Collections;
using UnityEditor;

public class MoveTrigger : MonoBehaviour
{
	public bool debug = false;
	public Vector3 locationA, locationB;
	[SerializeField] private AnimationCurve movementCurve;
	[SerializeField] private bool startAtA = true;
	[SerializeField] private float transitionTime = 1f;
	[SerializeField] private bool useUnscaledDeltaTime = false;
	[SerializeField] protected bool useWorldSpace = false;
	private bool isMoving = false;

	[SerializeField] protected Transform tr;

	public void Move(bool goToA)
	{
		startAtA = !goToA;
		if (!gameObject.activeSelf)
		{
			SetPosition(GetInterpolatedPosition(1f));
			return;
		}
		if (isMoving)
		{
			SetPosition(GetInterpolatedPosition(1f));
			StopAllCoroutines();
		}
		StartCoroutine(MoveCurve());
	}

	public void InstantMove(bool goToA)
	{
		startAtA = !goToA;
		SetPosition(GetInterpolatedPosition(1f));
	}

	private IEnumerator MoveCurve()
	{
		isMoving = true;
		float time = 0f;
		while (time < transitionTime)
		{
			time += useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
			float delta = time / transitionTime;
			float evaluation = movementCurve.Evaluate(delta);
			SetPosition(GetInterpolatedPosition(evaluation));
			yield return null;
		}
		isMoving = false;
	}

	private Vector3 GetInterpolatedPosition(float delta)
	{
		Vector3 start = startAtA ? locationA : locationB;
		Vector3 end = startAtA ? locationB : locationA;
		return Vector3.LerpUnclamped(start, end, delta);
	}

	protected virtual void SetPosition(Vector3 pos)
	{
		if (useWorldSpace)
		{
			tr.position = pos;
		}
		else
		{
			tr.localPosition = pos;
		}
	}
}