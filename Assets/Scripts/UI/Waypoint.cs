using UnityEngine;

public struct Waypoint
{
	public Transform target;
	public Vector2 position;

	public Waypoint(Transform target = null, Vector2? position = null)
	{
		this.target = target;
		this.position = position ?? Vector2.zero;
	}

	public Vector2 GetPosition()
	{
		return target?.position ?? position;
	}
}
