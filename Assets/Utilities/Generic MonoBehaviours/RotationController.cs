using UnityEngine;

public class RotationController : MonoBehaviour
{
	[SerializeField] private Vector3 rotationOffset;

	private Transform Tr => transform;

	private Vector3 Position => Tr.position;

	public Vector3 Rotation
	{
		get => Tr.eulerAngles;
		private set => Tr.eulerAngles = value;
	}

	public void PointTowards(Transform target)
	{
		PointTowards(target.position);
	}

	public void PointTowards(Vector3 targetPos)
	{
		Vector3 directionToPosition = (targetPos - Position).normalized;
		Vector3 rotateTowards = Quaternion.LookRotation(directionToPosition)
			.eulerAngles;
		SetRotation(rotateTowards);
	}

	public void SetRotation(Vector3 rotation)
	{
		SetXRotation(rotation.x);
		SetYRotation(rotation.y);
		SetZRotation(rotation.z);
	}

	public void SetXRotation(float angle)
	{
		Vector3 rot = Rotation;
		rot.x = angle + rotationOffset.x;
		Rotation = rot;
	}

	public void SetYRotation(float angle)
	{
		Vector3 rot = Rotation;
		rot.y = angle + rotationOffset.y;
		Rotation = rot;
	}

	public void SetZRotation(float angle)
	{
		Vector3 rot = Rotation;
		rot.z = angle + rotationOffset.z;
		Rotation = rot;
	}
}
