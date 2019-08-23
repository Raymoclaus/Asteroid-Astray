using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetData
{
	private List<Room> rooms = new List<Room>();
	public AreaType areaType = AreaType.Rocky;

	public Room AddRoom(RoomType type, int x, int y)
	{
		Room room = new Room(type, new Vector2Int(x, y));
		rooms.Add(room);
		return room;
	}

	public Room AddRoom(RoomType type, Vector2Int position)
	{
		Room room = new Room(type, position);
		rooms.Add(room);
		return room;
	}

	public List<Room> GetRooms() => rooms;

	public int GetRoomCount() => rooms.Count;

	public Room GetRoomAtPosition(Vector2Int position)
	{
		int index = GetIndexOfRoomAtPosition(position);
		return index == -1 ? null : rooms[index];
	}

	private int GetIndexOfRoomAtPosition(Vector2Int position)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].position == position) return i;
		}
		return -1;
	}
}
