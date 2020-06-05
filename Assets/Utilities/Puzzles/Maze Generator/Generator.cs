using CustomDataTypes;
using System.Collections.Generic;

namespace Puzzles.Maze
{
	public class Generator
	{
		public GridMatrix GeneratePuzzle(IntPair size, IntPair[] exits, int pathWidth)
		{
			GridMatrix maze = new GridMatrix(size, exits, pathWidth, false);
			IntPair currentSpot = exits[0];
			List<IntPair> path = new List<IntPair>();
			path.Add(currentSpot);
			maze.VisitExit(exits[0]);

			do
			{
				IntPair nextSpot = maze.IsNearTileAdjacentToUnvisitedExit(currentSpot);
				if (nextSpot == IntPair.one * -1)
				{
					nextSpot = maze.GetRandomSurroundingSoftWall(currentSpot);
				}

				if (nextSpot != IntPair.one * -1)
				{
					maze.Set(nextSpot, false);
					currentSpot = nextSpot;
					path.Add(currentSpot);

					IntPair nearbyExit = maze.NearbyUnvisitedExit(currentSpot);
					if (nearbyExit != IntPair.one * -1)
					{
						currentSpot = nearbyExit;
						maze.VisitExit(nearbyExit);
						path.Add(currentSpot);
					}
				}
				else if (path.Count > 0)
				{
					if (path.Count > (maze.GetLongestPath()?.Count ?? 0))
					{
						maze.SetLongestPath(new List<IntPair>(path));
					}
					path.RemoveAt(path.Count - 1);
					if (path.Count > 0)
					{
						currentSpot = path[path.Count - 1];
					}
				}
			} while (path.Count > 0);

			return maze;
		}
	}
}
