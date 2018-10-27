using UnityEngine;

public class DirectionMarker : MonoBehaviour
{
	[SerializeField]
	private float radius = 0.5f;
	private Transform parent, followTarget;
	private Vector2 locationTarget;

	private void Awake()
	{
		parent = transform.parent;
	}

	private void Update()
	{
		//get angle of current position to target position in degrees
		float angle = GetAngle();
		//rotate transform by angle
		transform.eulerAngles = Vector3.back * angle;
		//place transform at the current position relative to the parent
		angle *= Mathf.Deg2Rad;
		transform.position = parent.position + new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * radius;
	}

	private float GetAngle()
	{
		float angle;
		if (followTarget != null)
		{
			angle = -Vector2.SignedAngle(Vector2.up, followTarget.position - parent.position);
		}
		else
		{
			angle = -Vector2.SignedAngle(Vector2.up, locationTarget - (Vector2)parent.position);
		}
		return angle;
	}

	public void SetFollowTarget(Transform follow)
	{
		followTarget = follow;
	}

	public void SetLocationTarget(Vector2 location)
	{
		followTarget = null;
		locationTarget = location;
	}
}