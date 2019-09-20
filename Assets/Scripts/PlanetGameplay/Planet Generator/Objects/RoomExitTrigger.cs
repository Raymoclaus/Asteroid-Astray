using UnityEngine;
using TileLightsPuzzle;
using BlockPushPuzzle;

public class RoomExitTrigger : RoomObject
{
	public Direction direction;

	public RoomExitTrigger(Room room, Direction direction)
	{
		this.room = room;
		room.OnChangeExitPosition += AdjustPosition;
		room.OnTileLightsAdded += LockWithTileGrid;
		room.OnBlockPushAdded += LockWithBlockPush;
		this.direction = direction;
	}

	private void LockWithTileGrid(TileGrid tileGrid)
	{
		Room leadingRoom = room.GetRoom(direction);
		if (leadingRoom != null
			&& leadingRoom != room.previousRoom
			&& !room.IsLocked(direction))
		{
			room.LockWithoutKey(direction);
			tileGrid.OnPuzzleCompleted += UnlockExit;
		}
	}

	private void LockWithBlockPush(PushPuzzle puzzle)
	{
		Room leadingRoom = room.GetRoom(direction);
		if (leadingRoom != null
			&& leadingRoom != room.previousRoom
			&& !room.IsLocked(direction))
		{
			room.LockWithoutKey(direction);
			puzzle.OnPuzzleCompleted += UnlockExit;
		}
	}

	private void UnlockExit() => room.Unlock(direction);

	private void AdjustPosition(Direction direction, Vector2Int position)
	{
		if (this.direction != direction) return;
		SetPosition(position);
	}

	public override ObjType GetObjectType() => ObjType.ExitTrigger;
}
