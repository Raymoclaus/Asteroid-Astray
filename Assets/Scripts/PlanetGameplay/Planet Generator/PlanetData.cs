using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlanetData
{
	public string planetName = "Default Planet Name";
	private List<Room> rooms = new List<Room>();
	public Room startRoom, finalRoom;
	public AreaType areaType = AreaType.Cave;

	public Room AddRoom(RoomType type, int x, int y, Room previousRoom,
		PlanetDifficultyModifiers difficultyModifiers)
	{
		Room room = new Room(type, new IntPair(x, y),
			previousRoom, difficultyModifiers);
		rooms.Add(room);
		return room;
	}

	public Room AddRoom(RoomType type, IntPair position, Room previousRoom,
		PlanetDifficultyModifiers difficultyModifiers)
	{
		Room room = new Room(type, position, previousRoom,
			difficultyModifiers);
		rooms.Add(room);
		return room;
	}

	public List<Room> GetRooms() => rooms;

	public int GetRoomCount() => rooms.Count;

	public Room GetRoomAtPosition(IntPair position)
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

	public void Save()
		=> SaveLoad.Save(planetName, this);
}
