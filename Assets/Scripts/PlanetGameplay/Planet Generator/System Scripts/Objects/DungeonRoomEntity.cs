using UnityEngine;

public class DungeonRoomEntity : DungeonRoomObject
{
	public DungeonRoomEntity(DungeonRoom currentRoom, Vector2 position,
		string objectName, object objectData, bool isPersistent)
		: base(currentRoom, position, objectName, objectData, isPersistent)
	{
	}
}
