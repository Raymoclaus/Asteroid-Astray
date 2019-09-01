using UnityEngine;

public class RoomLock : RoomObject
{
	public RoomKey.KeyColour colour;
	public Direction direction;

	public RoomLock(Room room, RoomKey.KeyColour colour, Direction direction)
	{
		this.room = room;
		room.OnChangeExitPosition += AdjustPosition;
		this.colour = colour;
		this.direction = direction;
	}

	private void AdjustPosition(Direction direction, Vector2Int position)
	{
		if (this.direction != direction) return;
		this.position = position;
	}

	public void SetDirection(Direction direction) => this.direction = direction;

	public void Unlock() => room.Unlock(direction);

	public override ObjType GetObjectType() => ObjType.Lock;
}
