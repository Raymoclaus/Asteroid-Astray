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

	public Vector2 GetPosition() => target == null ? position : (Vector2)target.position;

	public static bool operator ==(Waypoint a, Waypoint b)
		=> a.target == b.target || a.GetPosition() == b.GetPosition();

	public static bool operator !=(Waypoint a, Waypoint b)
		=> a.target != b.target || a.GetPosition() != b.GetPosition();

	public override bool Equals(object obj) => base.Equals(obj);

	public override int GetHashCode() => base.GetHashCode();
}
