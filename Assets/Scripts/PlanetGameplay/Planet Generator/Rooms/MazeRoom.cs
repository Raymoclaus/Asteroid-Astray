using System.Collections.Generic;
using UnityEngine;
using MazePuzzle;

public class MazeRoom : Room
{
	Maze maze;
	private IntPair idealLootPosition;

	public MazeRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public MazeRoom(IntPair position, Room previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		MazeGenerator gen = new MazeGenerator();
		IntPair roomSize = InnerDimensions;
		IntPair[] exits = new IntPair[ExitCount];

		int count = 0;
		Direction[] directions = (Direction[])System.Enum.GetValues(typeof(Direction));
		for (int i = 0; i < directions.Length; i++)
		{
			Direction dir = directions[i];
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
				AddTile(tilePos, wall ? RoomTile.TileType.Wall : RoomTile.TileType.Floor);
			}
		}
	}

	private void SetUpLoot()
	{
		GetObjects<RoomKey>().ForEach(t => t.SetPosition(idealLootPosition));
	}
}
