using UnityEngine;

public class FollowTransform : MonoBehaviour
{
	public Transform followTarget;
	[Tooltip("Selecting false will allow the object to follow at a constant speed.")]
	public bool lerpMovement = true;
	public float speed = 1f;
	public Vector3 offset;

	private void Update()
	{
		if (followTarget == null) return;

		Vector3 targetPos = followTarget.position + offset;		
		if (lerpMovement)
		{
			transform.position = Vector3.Lerp(transform.position, targetPos, speed);
		}
		else
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPos, speed);
		}
	}
}