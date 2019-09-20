using System.Collections.Generic;
using UnityEngine;
using TileLightsPuzzle;
using BlockPushPuzzle;

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
	public bool hidden;

	public delegate void RevealedEventHandler();
	public event RevealedEventHandler OnKeyRevealed;

	public RoomKey(Room room, KeyColour colour)
	{
		this.room = room;
		room.OnMazeAdded += FindNewPlaceInMaze;
		room.OnTileLightsAdded += FindNewPlaceForTileLightsPuzzle;
		room.OnBlockPushAdded += FindNewPlaceForBlockPushPuzzle;
		this.colour = colour;
	}

	private void FindNewPlaceInMaze(MazePuzzle.Maze maze)
	{
		List<Vector2Int> path = maze.GetLongestPath();
		SetPosition(path[path.Count - 1]);
	}

	private void FindNewPlaceForTileLightsPuzzle(TileGrid tileGrid)
	{
		Vector2Int roomCenter = room.GetCenterInt();
		int puzzleHeight = tileGrid.GridSize.y;
		SetPosition(new Vector2Int(roomCenter.x, roomCenter.y + puzzleHeight / 2 + 1));
		hidden = true;
		tileGrid.OnPuzzleCompleted += RevealKey;
	}

	private void FindNewPlaceForBlockPushPuzzle(PushPuzzle puzzle)
	{
		Vector2Int roomCenter = room.GetCenterInt();
		int puzzleHeight = puzzle.GridSize.y;
		SetPosition(new Vector2Int(roomCenter.x, roomCenter.y + puzzleHeight / 2 - 1));
		hidden = true;
		puzzle.OnPuzzleCompleted += RevealKey;
	}

	private void RevealKey()
	{
		hidden = false;
		OnKeyRevealed?.Invoke();
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
