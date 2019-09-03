using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MazePuzzle
{
	public class MazeGenerator
	{
		public Maze GeneratePuzzle(Vector2Int size, Vector2Int[] exits)
		{
			Maze maze = new Maze(size, exits);
			Vector2Int currentSpot = exits[0];
			List<Vector2Int> path = new List<Vector2Int>();
			path.Add(currentSpot);
			maze.VisitExit(exits[0]);

			do
			{
				Vector2Int nextSpot = maze.GetRandomSurroundingSoftWall(currentSpot);

				if (nextSpot != Vector2Int.one * -1)
				{
					maze.Set(nextSpot, false);
					currentSpot = nextSpot;
					path.Add(currentSpot);

					Vector2Int nearbyExit = maze.NearbyUnvisitedExit(currentSpot);
					if (nearbyExit != Vector2Int.one * -1)
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
						maze.SetLongestPath(new List<Vector2Int>(path));
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
