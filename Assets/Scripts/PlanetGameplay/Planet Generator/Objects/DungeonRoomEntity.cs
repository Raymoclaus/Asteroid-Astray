using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomEntity : DungeonRoomObject
{
	public DungeonRoomEntity(DungeonRoom currentRoom, Vector2 position,
		string objectName, object objectData)
		: base(currentRoom, position, objectName, objectData)
	{
	}
}
