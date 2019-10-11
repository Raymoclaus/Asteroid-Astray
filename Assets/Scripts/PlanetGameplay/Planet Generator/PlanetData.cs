using System.Collections.Generic;

public class PlanetData
{
	public string planetName = "Default Planet Name";
	private List<DungeonRoom> rooms = new List<DungeonRoom>();
	IntPair startRoomPosition, finalRoomPosition;
	public DungeonRoom startRoom, finalRoom;
	public string areaType = "Cave";

	public PlanetData() { }

	public void AddRoom(DungeonRoom room) => rooms.Add(room);

	public List<DungeonRoom> GetRooms() => rooms;

	public int GetRoomCount() => rooms.Count;

	public DungeonRoom GetRoomAtPosition(IntPair position)
	{
		int index = GetIndexOfRoomAtPosition(position);
		return index == -1 ? null : rooms[index];
	}

	private int GetIndexOfRoomAtPosition(IntPair position)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].position == position) return i;
		}
		return -1;
	}
}
