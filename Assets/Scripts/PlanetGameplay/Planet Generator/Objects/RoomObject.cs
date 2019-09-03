using UnityEngine;

public abstract class RoomObject
{
	[HideInInspector] public Vector2Int position;
	protected Room room;

	public virtual ObjType GetObjectType() => ObjType.None;

	public enum ObjType
	{
		None,
		Lock,
		Key,
		ExitTrigger,
		LandingPad,
		TileLight
	}
}
