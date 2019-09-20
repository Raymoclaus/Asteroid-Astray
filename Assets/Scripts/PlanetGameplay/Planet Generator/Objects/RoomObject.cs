using UnityEngine;

public abstract class RoomObject
{
	private Vector2Int position;
	protected Room room;

	public delegate void PositionUpdatedEventHandler(Vector2Int position);
	public event PositionUpdatedEventHandler OnPositionUpdated;

	public Vector2Int GetPosition() => position;
	public void SetPosition(Vector2Int position)
	{
		if (this.position == position) return;
		this.position = position;
		OnPositionUpdated?.Invoke(position);
	}

	public virtual ObjType GetObjectType() => ObjType.None;

	public void LogPosition(Vector2Int pos) => Debug.Log(pos);

	public enum ObjType
	{
		None,
		Lock,
		Key,
		ExitTrigger,
		LandingPad,
		TileLight,
		PushableBlock,
		Player,
		GreenGroundButton,
		RedGroundButton,
		Dummy
	}
}
