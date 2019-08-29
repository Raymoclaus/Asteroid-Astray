using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MazeGenerator
{
	public class Generator
	{
		public Maze Generate(Vector2Int size, Vector2Int[] exits)
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
					path.RemoveAt(path.Count - 1);
					if (path.Count > 0)
					{
						currentSpot = path[path.Count - 1];
					}
				}
			} while (path.Count > 0);

			return maze;
		}

		public Maze Stretch(Maze toStretch, Vector2Int size)
		{
			Vector2Int[] exits = toStretch.GetExits();
			Vector2Int originalSize = toStretch.GetSize();
			for (int i = 0; i < exits.Length; i++)
			{
				if (exits[i].x == 1)
				{
					exits[i].y = Mathf.CeilToInt((float)exits[i].y / originalSize.y * size.y);
				}
				if (exits[i].y == 1)
				{
					exits[i].x = Mathf.CeilToInt((float)exits[i].x / originalSize.x * size.x);
				}
				if (exits[i].x == originalSize.x - 2)
				{
					exits[i].x = size.x - 2;
					exits[i].y = Mathf.CeilToInt((float)exits[i].y / originalSize.y * size.y);
				}
				if (exits[i].y == originalSize.y - 2)
				{
					exits[i].y = size.y - 2;
					exits[i].x = Mathf.CeilToInt((float)exits[i].x / originalSize.x * size.x);
				}
			}

			//stretch the maze
			Maze newMaze = new Maze(size, exits, true);

			for (int i = 0; i < exits.Length; i++)
			{
				newMaze.VisitExit(exits[i]);
			}

			for (int x = 0; x < originalSize.x; x++)
			{
				for (int y = 0; y < originalSize.y; y++)
				{
					Vector2Int pos = new Vector2Int(x, y);
					int index = toStretch.Index(pos);
					if (!toStretch.Get(index)) continue;

					Vector2Int stretchedPos = StretchPosition(pos, originalSize, size);
					newMaze.Set(stretchedPos, true);
				}
			}

			//fill in the gaps
			newMaze = FillInGaps(newMaze);

			return newMaze;
		}

		public Maze DoubleSize(Maze maze)
		{
			Vector2Int[] exits = maze.GetExits();
			Vector2Int originalSize = maze.GetSize();
			Vector2Int size = new Vector2Int(originalSize.x * 2, originalSize.y * 2);
			for (int i = 0; i < exits.Length; i++)
			{
				if (exits[i].x == 1)
				{
					exits[i].y = Mathf.CeilToInt((float)exits[i].y / originalSize.y * size.y);
				}
				if (exits[i].y == 1)
				{
					exits[i].x = Mathf.CeilToInt((float)exits[i].x / originalSize.x * size.x);
				}
				if (exits[i].x == originalSize.x - 2)
				{
					exits[i].x = size.x - 2;
					exits[i].y = Mathf.CeilToInt((float)exits[i].y / originalSize.y * size.y);
				}
				if (exits[i].y == originalSize.y - 2)
				{
					exits[i].y = size.y - 2;
					exits[i].x = Mathf.CeilToInt((float)exits[i].x / originalSize.x * size.x);
				}
			}
			Maze newMaze = new Maze(size, exits, true);

			for (int i = 0; i < exits.Length; i++)
			{
				newMaze.VisitExit(exits[i]);
			}

			for (int x = 0; x < originalSize.x; x++)
			{
				for (int y = 0; y < originalSize.y; y++)
				{
					Vector2Int pos = new Vector2Int(x, y);
					int index = maze.Index(pos);
					if (!maze.Get(index)) continue;

					Vector2Int stretchedPos = pos * 2;
					for (int i = 0; i < 2; i++)
					{
						for (int j = 0; j < 2; j++)
						{
							int stretchedIndex = newMaze.Index(stretchedPos.x + i, stretchedPos.y + j);
							newMaze.Set(stretchedPos.x + i, stretchedPos.y + j, true);
						}
					}
				}
			}

			return newMaze;
		}

		private Maze FillInGaps(Maze maze)
		{
			Vector2Int size = maze.GetSize();
			for (int i = 0; i < maze.ArrayLength(); i++)
			{
				Vector2Int pos = maze.GetPos(i);
				Debug.Log(pos);

				if (!maze.IsWall(pos))
				{
					if (pos.y != 0 && pos.y != size.y - 1
						&& maze.IsWall(pos.x, pos.y + 1) && maze.IsWall(pos.x, pos.y - 1))
					{
						maze.Set(pos, true);
					}
					if (pos.x != 0 && pos.x != size.x - 1
						&& maze.IsWall(pos.x + 1, pos.y) && maze.IsWall(pos.x - 1, pos.y))
					{
						maze.Set(pos, true);
					}
				}
			}
			return maze;
		}

		public Vector2Int StretchPosition(Vector2Int pos, Vector2Int sizeA, Vector2Int sizeB)
		{
			pos.x = Mathf.CeilToInt((float)pos.x / sizeA.x * sizeB.x);
			pos.y = Mathf.CeilToInt((float)pos.y / sizeA.y * sizeB.y);
			return pos;
		}
	}
}
