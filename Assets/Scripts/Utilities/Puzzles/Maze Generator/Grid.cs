using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Puzzles.Maze
{
	public class Grid
	{
		private bool[] walls;
		private IntPair size;
		private IntPair[] exits;
		private int pathWidth;
		private List<IntPair> visitedExits = new List<IntPair>();
		private List<IntPair> longestPath;

		public Grid(IntPair size, IntPair[] exits, int pathWidth, bool startEmpty)
		{
			this.size = size;
			walls = new bool[size.x * size.y];
			this.exits = exits;
			this.pathWidth = pathWidth;
			if (startEmpty) return;
			for (int i = 0; i < walls.Length; i++)
			{
				walls[i] = true;
			}
			for (int i = 0; i < exits.Length; i++)
			{
				Set(exits[i], false);
			}
		}

		//converting x, y position to index that accesses the array
		public int Index(IntPair pos) => Index(pos.x, pos.y);
		public int Index(int x, int y) => y * size.x + x;

		//returns whether that position is a wall or not
		public bool IsWall(IntPair pos) => IsWall(pos.x, pos.y);
		public bool IsWall(int x, int y)
		{
			for (int i = 0; i < pathWidth; i++)
			{
				for (int j = 0; j < pathWidth; j++)
				{
					IntPair pos = new IntPair(x + i, y + j);
					int index = Index(pos);
					if (index < 0
						|| index >= ArrayLength()
						|| walls[index]
						|| IsUnvisitedExit(pos)) return true;
				}
			}
			return false;
		}

		//returns the amount of walls adjacent to the spot
		public int SurroundingWallCount(IntPair pos) => SurroundingWallCount(pos.x, pos.y);
		public int SurroundingWallCount(int x, int y)
		{
			int count = 0;
			if (IsWall(x, y + pathWidth)) count++;
			if (IsWall(x + pathWidth, y)) count++;
			if (IsWall(x, y - pathWidth)) count++;
			if (IsWall(x - pathWidth, y)) count++;
			return count;
		}

		//returns the amount of walls diagonal to the spot
		public int SurroundingDiagonalWallCount(IntPair pos)
			=> SurroundingDiagonalWallCount(pos.x, pos.y);
		public int SurroundingDiagonalWallCount(int x, int y)
		{
			int count = 0;
			if (IsWall(x + pathWidth, y + pathWidth)) count++;
			if (IsWall(x + pathWidth, y - pathWidth)) count++;
			if (IsWall(x - pathWidth, y - pathWidth)) count++;
			if (IsWall(x - pathWidth, y + pathWidth)) count++;
			return count;
		}

		//returns the amount of walls surrounding the spot including diagonals
		public int SurroundingEightWallCount(IntPair pos)
			=> SurroundingEightWallCount(pos.x, pos.y);
		public int SurroundingEightWallCount(int x, int y)
			=> SurroundingWallCount(x, y) + SurroundingDiagonalWallCount(x, y);

		//returns whether the spot is on the edge of the maze
		public bool IsOuterWall(IntPair pos) => IsOuterWall(pos.x, pos.y);
		public bool IsOuterWall(int x, int y)
		{
			for (int i = 0; i < pathWidth; i++)
			{
				for (int j = 0; j < pathWidth; j++)
				{
					if (x + i <= 0
					   || y + j <= 0
					   || x + i >= size.x - 1
					   || y + j >= size.y - 1) return true;
				}
			}
			return false;
		}

		//returns whether the spot is an exit point
		public bool IsExit(IntPair pos) => IsExit(pos.x, pos.y);
		public bool IsExit(int x, int y)
		{
			for (int i = 0; i < exits.Length; i++)
			{
				for (int j = 0; j < pathWidth; j++)
				{
					for (int k = 0; k < pathWidth; k++)
					{
						if (exits[i].x == x + j && exits[i].y == y + k) return true;
					}
				}
			}
			return false;
		}

		//returns whether the spot is an unvisited exit point
		public bool IsUnvisitedExit(IntPair pos) => IsUnvisitedExit(pos.x, pos.y);
		public bool IsUnvisitedExit(int x, int y)
		{
			if (!IsExit(x, y)) return false;

			for (int i = 0; i < visitedExits.Count; i++)
			{
				for (int j = 0; j < pathWidth; j++)
				{
					for (int k = 0; k < pathWidth; k++)
					{
						if (visitedExits[i].x == x + j
							&& visitedExits[i].y == y + k) return false;
					}
				}
			}
			return true;
		}

		//returns whether the spot is a wall that cannot be passed through
		public bool IsHardWall(IntPair pos) => IsHardWall(pos.x, pos.y);
		public bool IsHardWall(int x, int y)
		{
			if (IsOuterWall(x, y)) return true;
			if (!IsWall(x, y)) return false;

			if (SurroundingEightWallCount(x, y) < 6
				|| SurroundingWallCount(x, y) < 3) return true;


			//additional rule to avoid diagonal hard walls
			//if a diagonal space is not a wall
			//...AND the two common adjacent spaces ARE walls...
			//treat it as a hard wall regardless of surrounding wall count

			bool upLeft = IsWall(x - pathWidth, y + pathWidth);
			bool up = IsWall(x, y + pathWidth);
			bool upRight = IsWall(x + pathWidth, y + pathWidth);
			bool right = IsWall(x + pathWidth, y);
			bool downRight = IsWall(x + pathWidth, y - pathWidth);
			bool down = IsWall(x, y - pathWidth);
			bool downLeft = IsWall(x - pathWidth, y - pathWidth);
			bool left = IsWall(x - pathWidth, y);

			//check top right
			if (!upRight)
			{
				//top and right
				if (up && right) return true;
			}

			//check bottom right
			if (!downRight)
			{
				//bottom and right
				if (down && right) return true;
			}

			//check bottom left
			if (!downLeft)
			{
				//bottom and left
				if (down && left) return true;
			}

			//check top left
			if (!upLeft)
			{
				//top and left
				if (up && left) return true;
			}

			return false;
		}

		//returns whether the spot is a wall but doesn't meet the requirements of being a hard wall
		public bool IsSoftWall(IntPair pos) => IsSoftWall(pos.x, pos.y);
		public bool IsSoftWall(int x, int y) => !IsHardWall(x, y) && IsWall(x, y);

		//returns a random position adjacent to pos that is considered a soft wall
		//return (-1, -1) if no soft wall is found
		public IntPair GetRandomSurroundingSoftWall(IntPair pos)
			=> GetRandomSurroundingSoftWall(pos.x, pos.y);
		public IntPair GetRandomSurroundingSoftWall(int x, int y)
		{
			List<IntPair> arr = new List<IntPair>();
			IntPair rightPos = new IntPair(x + pathWidth, y);
			if (IsSoftWall(rightPos)) arr.Add(rightPos);
			IntPair leftPos = new IntPair(x - pathWidth, y);
			if (IsSoftWall(leftPos)) arr.Add(leftPos);
			IntPair upPos = new IntPair(x, y + pathWidth);
			if (IsSoftWall(upPos)) arr.Add(upPos);
			IntPair downPos = new IntPair(x, y - pathWidth);
			if (IsSoftWall(downPos)) arr.Add(downPos);

			if (arr.Count == 0) return IntPair.one * -1;

			int randomIndex = Random.Range(0, arr.Count);
			return arr[randomIndex];
		}

		public IntPair IsNearTileAdjacentToUnvisitedExit(IntPair pos)
		{
			IntPair rightPos = new IntPair(pos.x + pathWidth, pos.y);
			IntPair leftPos = new IntPair(pos.x - pathWidth, pos.y);
			IntPair upPos = new IntPair(pos.x, pos.y + pathWidth);
			IntPair downPos = new IntPair(pos.x, pos.y - pathWidth);

			if (NearbyUnvisitedExit(rightPos) != IntPair.one * -1) return rightPos;
			if (NearbyUnvisitedExit(leftPos) != IntPair.one * -1) return leftPos;
			if (NearbyUnvisitedExit(upPos) != IntPair.one * -1) return upPos;
			if (NearbyUnvisitedExit(downPos) != IntPair.one * -1) return downPos;

			return IntPair.one * -1;
		}

		//returns the position of an adjacent unvisited exit
		//if no valid target is found, returns (-1, -1)
		public IntPair NearbyUnvisitedExit(IntPair pos) => NearbyUnvisitedExit(pos.x, pos.y);
		public IntPair NearbyUnvisitedExit(int x, int y)
		{
			IntPair rightPos = new IntPair(x + pathWidth, y);
			if (IsUnvisitedExit(rightPos)) return rightPos;
			IntPair leftPos = new IntPair(x - pathWidth, y);
			if (IsUnvisitedExit(leftPos)) return leftPos;
			IntPair upPos = new IntPair(x, y + pathWidth);
			if (IsUnvisitedExit(upPos)) return upPos;
			IntPair downPos = new IntPair(x, y - pathWidth);
			if (IsUnvisitedExit(downPos)) return downPos;
			return IntPair.one * -1;
		}

		//sets an exit to be treated as "visited"
		public void VisitExit(IntPair pos) => VisitExit(pos.x, pos.y);
		public void VisitExit(int x, int y)
		{
			if (!IsExit(x, y) || !IsUnvisitedExit(x, y)) return;
			visitedExits.Add(new IntPair(x, y));
		}

		//sets the position in the maze to be a wall
		public void Set(IntPair pos, bool wall) => Set(pos.x, pos.y, wall);
		public void Set(int x, int y, bool wall)
		{
			for (int i = 0; i < pathWidth; i++)
			{
				for (int j = 0; j < pathWidth; j++)
				{
					walls[Index(x + i, y + j)] = wall;
				}
			}
		}

		public bool Get(int index) => walls[index];

		public int ArrayLength() => walls.Length;

		public IntPair GetSize() => size;

		public IntPair GetPos(int index) => new IntPair(index % size.x, index / size.x);

		public IntPair[] GetExits() => exits;

		public void SetLongestPath(List<IntPair> path) => longestPath = path;

		public List<IntPair> GetLongestPath() => longestPath;
	}
}
