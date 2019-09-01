using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomExitTrigger : RoomObject
{
	public Direction direction;

	public RoomExitTrigger(Room room, Direction direction)
	{
		this.room = room;
		room.OnChangeExitPosition += AdjustPosition;
		this.direction = direction;
	}

	private void AdjustPosition(Direction direction, Vector2Int position)
	{
		if (this.direction != direction) return;
		this.position = position;
	}

	public override ObjType GetObjectType() => ObjType.ExitTrigger;
}
