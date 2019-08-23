using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomExitTrigger : RoomObject
{
	public Direction direction;

	public RoomExitTrigger(Direction direction)
	{
		this.direction = direction;
	}

	public override ObjType GetObjectType() => ObjType.ExitTrigger;
}
