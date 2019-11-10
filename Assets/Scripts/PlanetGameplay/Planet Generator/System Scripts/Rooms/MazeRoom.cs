using System.Collections.Generic;
using UnityEngine;
using Puzzles.Maze;
using CustomDataTypes;

public class MazeRoom : DungeonRoom
{
	GridMatrix maze;
	private IntPair idealLootPosition;

	public MazeRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		Generator gen = new Generator();
		IntPair roomSize = InnerDimensions;
		IntPair[] exits = new IntPair[ExitCount];

		int count = 0;
		for (int i = 0; i < ChunkCoords.DIRECTION_COUNT; i++)
		{
			Direction dir = (Direction)i;
			if (HasExit(dir))
			{
				bool vertical = dir.IsVertical();
				IntPair exitPos = GetExitPos(dir);
				IntPair adjustedExitPos = exitPos;
				//left, right
				IntPair horizontalBoundaries = HorizontalBoundaries;
				horizontalBoundaries.y -= (ExitWidth - 1);
				//down, up
				IntPair verticalBoundaries = VerticalBoundaries;
				verticalBoundaries.y -= (ExitWidth - 1);
				adjustedExitPos.x = Mathf.Clamp(adjustedExitPos.x,
					horizontalBoundaries.x, horizontalBoundaries.y);
				adjustedExitPos.y = Mathf.Clamp(adjustedExitPos.y,
					verticalBoundaries.x, verticalBoundaries.y);
				exits[count] = adjustedExitPos;
				count++;
			}
		}

		maze = gen.GeneratePuzzle(roomSize, exits, ExitWidth);
		List<IntPair> path = maze.GetLongestPath();
		idealLootPosition = path != null ? path[path.Count - 1] : exits[0];

		SetUpLoot();

		for (int x = 1; x < maze.GetSize().x - 1; x++)
		{
			for (int y = 1; y < maze.GetSize().y - 1; y++)
			{
				IntPair tilePos = new IntPair(x, y);
				int index = maze.Index(x, y);
				bool wall = maze.Get(index);
				AddTile(tilePos,
					wall ? DungeonRoomTileType.Wall : DungeonRoomTileType.Floor);
			}
		}
	}

	private void SetUpLoot()
	{
		for (int i = 0; i < roomObjects.Count; i++)
		{
			DungeonRoomObject obj = roomObjects[i];
			if (obj.ObjectName == "Key")
			{
				obj.RoomSpacePosition = idealLootPosition;
			}
		}
	}
}
