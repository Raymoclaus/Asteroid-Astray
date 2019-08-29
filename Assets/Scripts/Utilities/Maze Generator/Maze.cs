using System.Collections.Generic;
using UnityEngine;

namespace MazeGenerator
{
	public class Maze
	{
		private bool[] walls;
		private Vector2Int size;
		private Vector2Int[] exits;
		private List<Vector2Int> visitedExits = new List<Vector2Int>();

		public Maze(Vector2Int size, Vector2Int[] exits, bool startEmpty = false)
		{
			this.size = size;
			walls = new bool[size.x * size.y];
			this.exits = exits;
			if (startEmpty) return;
			for (int i = 0; i < walls.Length; i++)
			{
				walls[i] = !IsExit(GetPos(i));
			}
		}

		//converting x, y position to index that accesses the array
		public int Index(Vector2Int pos) => Index(pos.x, pos.y);
		public int Index(int x, int y) => y * size.x + x;

		//returns whether that position is a wall or not
		public bool IsWall(Vector2Int pos) => IsWall(pos.x, pos.y);
		public bool IsWall(int x, int y) => walls[Index(x, y)] || IsUnvisitedExit(x, y);

		//returns the amount of walls adjacent to the spot
		public int SurroundingWallCount(Vector2Int pos) => SurroundingWallCount(pos.x, pos.y);
		public int SurroundingWallCount(int x, int y)
		{
			int count = 0;
			if (IsWall(x, y + 1)) count++;
			if (IsWall(x + 1, y)) count++;
			if (IsWall(x, y - 1)) count++;
			if (IsWall(x - 1, y)) count++;
			return count;
		}

		//returns the amount of walls diagonal to the spot
		public int SurroundingDiagonalWallCount(Vector2Int pos)
			=> SurroundingDiagonalWallCount(pos.x, pos.y);
		public int SurroundingDiagonalWallCount(int x, int y)
		{
			int count = 0;
			if (IsWall(x + 1, y + 1)) count++;
			if (IsWall(x + 1, y - 1)) count++;
			if (IsWall(x - 1, y - 1)) count++;
			if (IsWall(x - 1, y + 1)) count++;
			return count;
		}

		//returns the amount of walls surrounding the spot including diagonals
		public int SurroundingEightWallCount(Vector2Int pos)
			=> SurroundingEightWallCount(pos.x, pos.y);
		public int SurroundingEightWallCount(int x, int y)
			=> SurroundingWallCount(x, y) + SurroundingDiagonalWallCount(x, y);

		//returns whether the spot is on the edge of the maze
		public bool IsOuterWall(Vector2Int pos) => IsOuterWall(pos.x, pos.y);
		public bool IsOuterWall(int x, int y)
			=> x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1;

		//returns whether the spot is an exit point
		public bool IsExit(Vector2Int pos) => IsExit(pos.x, pos.y);
		public bool IsExit(int x, int y)
		{
			for (int i = 0; i < exits.Length; i++)
			{
				if (exits[i].x == x && exits[i].y == y) return true;
			}
			return false;
		}

		//returns whether the spot is an unvisited exit point
		public bool IsUnvisitedExit(Vector2Int pos) => IsUnvisitedExit(pos.x, pos.y);
		public bool IsUnvisitedExit(int x, int y)
		{
			if (!IsExit(x, y)) return false;

			for (int i = 0; i < visitedExits.Count; i++)
			{
				if (visitedExits[i].x == x && visitedExits[i].y == y) return false;
			}
			return true;
		}

		//returns whether the spot is a wall that cannot be passed through
		public bool IsHardWall(Vector2Int pos) => IsHardWall(pos.x, pos.y);
		public bool IsHardWall(int x, int y)
		{
			if (IsOuterWall(x, y)) return true;
			if (!IsWall(x, y)) return false;

			if (SurroundingEightWallCount(x, y) < 6 || SurroundingWallCount(x, y) < 3) return true;


			//additional rule to avoid diagonal hard walls
			//if a diagonal space is not a wall
			//...AND the two common adjacent spaces ARE walls...
			//treat it as a hard wall regardless of surrounding wall count

			//check top right
			if (!IsWall(x + 1, y + 1))
			{
				if (IsWall(x, y + 1) && IsWall(x + 1, y)) return true;
			}

			//check bottom right
			if (!IsWall(x + 1, y - 1))
			{
				//bottom and right
				if (IsWall(x, y - 1) && IsWall(x + 1, y)) return true;
			}

			//check bottom left
			if (!IsWall(x - 1, y - 1))
			{
				//bottom and left
				if (IsWall(x, y - 1) && IsWall(x - 1, y)) return true;
			}

			//check top left
			if (!IsWall(x - 1, y + 1))
			{
				//top and left
				if (IsWall(x, y + 1) && IsWall(x - 1, y)) return true;
			}

			return false;
		}

		//returns whether the spot is a wall but doesn't meet the requirements of being a hard wall
		public bool IsSoftWall(Vector2Int pos) => IsSoftWall(pos.x, pos.y);
		public bool IsSoftWall(int x, int y) => !IsHardWall(x, y) && IsWall(x, y);

		//returns a random position adjacent to pos that is considered a soft wall
		//return (-1, -1) if no soft wall is found
		public Vector2Int GetRandomSurroundingSoftWall(Vector2Int pos)
			=> GetRandomSurroundingSoftWall(pos.x, pos.y);
		public Vector2Int GetRandomSurroundingSoftWall(int x, int y)
		{
			List<Vector2Int> arr = new List<Vector2Int>();
			Vector2Int pos = new Vector2Int(x, y);
			pos.x++;
			if (IsSoftWall(pos)) arr.Add(pos);
			pos.x -= 2;
			if (IsSoftWall(pos)) arr.Add(pos);
			pos.x++;
			pos.y++;
			if (IsSoftWall(pos)) arr.Add(pos);
			pos.y -= 2;
			if (IsSoftWall(pos)) arr.Add(pos);

			if (arr.Count == 0) return Vector2Int.one * -1;

			int randomIndex = Random.Range(0, arr.Count);
			return arr[randomIndex];
		}

		//returns the position of an adjacent unvisited exit
		//if no valid target is found, returns (-1, -1)
		public Vector2Int NearbyUnvisitedExit(Vector2Int pos) => NearbyUnvisitedExit(pos.x, pos.y);
		public Vector2Int NearbyUnvisitedExit(int x, int y)
		{
			Vector2Int pos = new Vector2Int(x, y);
			pos.x++;
			if (IsUnvisitedExit(pos)) return pos;
			pos.x -= 2;
			if (IsUnvisitedExit(pos)) return pos;
			pos.x++;
			pos.y++;
			if (IsUnvisitedExit(pos)) return pos;
			pos.y -= 2;
			if (IsUnvisitedExit(pos)) return pos;
			return Vector2Int.one * -1;
		}

		//sets an exit to be treated as "visited"
		public void VisitExit(Vector2Int pos) => VisitExit(pos.x, pos.y);
		public void VisitExit(int x, int y)
		{
			if (!IsExit(x, y) || !IsUnvisitedExit(x, y)) return;
			visitedExits.Add(new Vector2Int(x, y));
		}

		//sets the position in the maze to be a wall
		public void Set(Vector2Int pos, bool wall) => Set(pos.x, pos.y, wall);
		public void Set(int x, int y, bool wall) => walls[Index(x, y)] = wall;

		public bool Get(int index) => walls[index];

		public int ArrayLength() => walls.Length;

		public Vector2Int GetSize() => size;

		public Vector2Int GetPos(int index) => new Vector2Int(index % size.x, index / size.x);

		public Vector2Int[] GetExits() => exits;
	}
}
