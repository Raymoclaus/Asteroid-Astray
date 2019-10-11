using UnityEngine;

public class DungeonRoomObject
{
	public DungeonRoomObject(DungeonRoom currentRoom, Vector3 roomSpacePosition, string objectName, object objectData)
	{
		CurrentRoom = currentRoom;
		RoomSpacePosition = roomSpacePosition;
		ObjectName = objectName;
		ObjectData = objectData;
	}

	public DungeonRoom CurrentRoom { get; set; }
	public Vector3 RoomSpacePosition { get; set; }
	public Vector3 Position => CurrentRoom.WorldSpacePosition + RoomSpacePosition;
	public string ObjectName { get; private set; }
	public object ObjectData { get; private set; }
}
