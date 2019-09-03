using System.Collections.Generic;
using UnityEngine;

public class RoomKey : RoomObject
{
	public enum KeyColour
	{
		Blue,
		Red,
		Yellow,
		Green
	}

	public KeyColour colour;

	public RoomKey(Room room, KeyColour colour)
	{
		this.room = room;
		room.OnMazeAdded += FindNewPlaceInMaze;
		this.colour = colour;
	}

	private void FindNewPlaceInMaze(MazePuzzle.Maze maze)
	{
		List<Vector2Int> path = maze.GetLongestPath();
		position = path[path.Count - 1];
	}

	public override ObjType GetObjectType() => ObjType.Key;

	public static Item.Type ConvertToItemType(KeyColour col)
	{
		switch (col)
		{
			default: return Item.Type.BlueKey;
			case KeyColour.Blue: return Item.Type.BlueKey;
			case KeyColour.Red: return Item.Type.RedKey;
			case KeyColour.Yellow: return Item.Type.YellowKey;
			case KeyColour.Green: return Item.Type.GreenKey;
		}
	}
}
