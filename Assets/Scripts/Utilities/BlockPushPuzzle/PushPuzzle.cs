using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPushPuzzle
{
	public class PushPuzzle
	{
		public delegate void PuzzleCompletedEventHandler();
		public event PuzzleCompletedEventHandler OnPuzzleCompleted;
		public delegate void BlockMovedEventHandler(
			IntPair startPos, IntPair direction, float time);
		public event BlockMovedEventHandler OnBlockMoved;

		public bool[] grid;
		private bool[] gridInitialState;
		public IntPair GridSize { get; private set; }
		public int padding;
		public IntPair finishTile;
		public IntPair[] resetTiles;
		public int solutionCount;
		private bool completed;
		private Stack<Change> playerChanges = new Stack<Change>();

		public PushPuzzle(IntPair size, int padding)
		{
			GridSize = size;
			this.padding = padding;
			grid = new bool[size.x * size.y];
			resetTiles = new IntPair[]
			{
				new IntPair(0, 0),
				new IntPair(GridSize.x - 1, 0),
				new IntPair(0, GridSize.y - 1),
				new IntPair(GridSize.x - 1, GridSize.y - 1)
			};
		}

		public void Reset()
		{
			SetAll(true);
			int minimumDimension = Mathf.Min(GridSize.x, GridSize.y);
			int range = padding + minimumDimension / 2 - 2;
			finishTile.x = Random.Range(range, GridSize.x - 1 - range);
			finishTile.y = Random.Range(range, GridSize.y - 1 - range);
			SetBlock(finishTile, false);
			solutionCount = 0;
		}

		public void FinishGeneration()
		{
			for (int x = padding; x < GridSize.x - padding; x++)
			{
				for (int y = padding; y < GridSize.y - padding; y++)
				{
					IntPair pos = new IntPair(x, y);
					if (IsNearOuterEdge(pos, 1) && !IsOnOuterEdge(pos))
					{
						SetBlock(pos, false);
					}
				}
			}
			SaveInitialState();
		}

		public void CompletePuzzle()
		{
			if (completed) return;

			completed = true;
			Debug.Log("Block Push Puzzle Completed");
			OnPuzzleCompleted?.Invoke();
		}

		public bool PushBlock(IntPair blockPosition, IntPair direction)
		{
			if (!BlockExists(blockPosition)) return false;
			IntPair pushPosition = blockPosition + direction;
			if (GetIndexFromPosition(pushPosition) == -1
				|| BlockExists(pushPosition)) return false;
			SetBlock(blockPosition, false);
			SetBlock(pushPosition, true);
			playerChanges.Push(new Change(blockPosition, direction));
			OnBlockMoved?.Invoke(blockPosition, direction, 0.5f);
			return true;
		}

		public bool BlockExists(IntPair position)
		{
			int index = GetIndexFromPosition(position);
			if (index == -1) return false;
			return grid[index];
		}

		public bool ListContainsAdjacentPosition(List<IntPair> list,
			IntPair pos, int range)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (IsAdjacentTo(list[i], pos, range)) return true;
			}
			return false;
		}

		public bool ListContainsNearPosition(List<IntPair> list,
			IntPair pos, int range)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (IsNearTo(list[i], pos, range)) return true;
			}
			return false;
		}

		private bool IsAdjacentTo(IntPair posA, IntPair posB, int range)
		{
			int totalDifference = Mathf.Abs(posA.x - posB.x)
				+ Mathf.Abs(posA.y - posB.y);
			return totalDifference <= range;
		}

		public bool IsNearTo(IntPair posA, IntPair posB, int range)
		{
			return Mathf.Abs(posA.x - posB.x) <= range
				&& Mathf.Abs(posA.y - posB.y) <= range;
		}

		private void SaveInitialState()
		{
			gridInitialState = new bool[grid.Length];
			for (int i = 0; i < grid.Length; i++)
			{
				gridInitialState[i] = grid[i];
			}
		}

		public void LoadInitialState()
		{
			//for (int i = 0; i < grid.Length; i++)
			//{
			//	grid[i] = gridInitialState[i];
			//}

			while (playerChanges.Count > 0)
			{
				RevertLastChange();
			}
		}

		public void RevertLastChange()
		{
			if (playerChanges.Count == 0) return;

			Change change = playerChanges.Pop();
			IntPair startPos = change.pos;
			IntPair direction = change.dir;
			IntPair newPos = startPos + direction;
			IntPair oppositeDirection = direction * -1;
			SetBlock(newPos, false);
			SetBlock(startPos, true);
			OnBlockMoved?.Invoke(newPos, oppositeDirection, 1f / 10f);
		}

		public void SetBlock(IntPair position, bool active)
		{
			int index = GetIndexFromPosition(position);
			if (index == -1) return;
			grid[index] = active;
		}

		public int GetIndexFromPosition(IntPair position)
		{
			if (IsOnOuterEdge(position)
				|| PositionOutOfBounds(position)) return -1;
			int index = position.y * GridSize.x + position.x;
			if (index <= 0 || index >= grid.Length) return -1;
			return index;
		}

		public IntPair GetPositionFromIndex(int index)
			=> new IntPair(index % GridSize.x, index / GridSize.x);

		private bool IsOnOuterEdge(IntPair position)
			=> IsNearOuterEdge(position, 0);

		public bool IsNearOuterEdge(IntPair position, int range)
		{
			int paddingRange = padding + range;
			return position.x < paddingRange
				|| position.y < paddingRange
				|| position.x >= GridSize.x - paddingRange
				|| position.y >= GridSize.y - paddingRange;
		}

		private bool PositionOutOfBounds(IntPair position)
		{
			return position.x < 0
				|| position.y < 0
				|| position.x >= GridSize.x
				|| position.y >= GridSize.y;
		}

		private void SetAll(bool active)
		{
			for (int i = 0; i < grid.Length; i++)
			{
				grid[i] = active;
			}
		}

		private struct Change
		{
			public IntPair pos, dir;

			public Change(IntPair pos, IntPair dir)
			{
				this.pos = pos;
				this.dir = dir;
			}
		}
	}
}
