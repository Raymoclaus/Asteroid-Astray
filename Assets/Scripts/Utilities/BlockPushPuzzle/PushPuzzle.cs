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
			Vector2Int startPos, Vector2Int direction, float time);
		public event BlockMovedEventHandler OnBlockMoved;

		public bool[] grid;
		private bool[] gridInitialState;
		public Vector2Int GridSize { get; private set; }
		public int padding;
		public Vector2Int finishTile;
		public Vector2Int[] resetTiles;
		public int solutionCount;
		private bool completed;
		private Stack<Change> playerChanges = new Stack<Change>();

		public PushPuzzle(Vector2Int size, int padding)
		{
			GridSize = size;
			this.padding = padding;
			grid = new bool[size.x * size.y];
			resetTiles = new Vector2Int[]
			{
				new Vector2Int(0, 0),
				new Vector2Int(GridSize.x - 1, 0),
				new Vector2Int(0, GridSize.y - 1),
				new Vector2Int(GridSize.x - 1, GridSize.y - 1)
			};
			Reset();
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
					Vector2Int pos = new Vector2Int(x, y);
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

		public bool PushBlock(Vector2Int blockPosition, Vector2Int direction)
		{
			if (!BlockExists(blockPosition)) return false;
			Vector2Int pushPosition = blockPosition + direction;
			if (GetIndexFromPosition(pushPosition) == -1
				|| BlockExists(pushPosition)) return false;
			SetBlock(blockPosition, false);
			SetBlock(pushPosition, true);
			playerChanges.Push(new Change(blockPosition, direction));
			OnBlockMoved?.Invoke(blockPosition, direction, 0.5f);
			return true;
		}

		public bool BlockExists(Vector2Int position)
		{
			int index = GetIndexFromPosition(position);
			if (index == -1) return false;
			return grid[index];
		}

		public bool ListContainsAdjacentPosition(List<Vector2Int> list,
			Vector2Int pos, int range)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (IsAdjacentTo(list[i], pos, range)) return true;
			}
			return false;
		}

		public bool ListContainsNearPosition(List<Vector2Int> list,
			Vector2Int pos, int range)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (IsNearTo(list[i], pos, range)) return true;
			}
			return false;
		}

		private bool IsAdjacentTo(Vector2Int posA, Vector2Int posB, int range)
		{
			int totalDifference = Mathf.Abs(posA.x - posB.x)
				+ Mathf.Abs(posA.y - posB.y);
			return totalDifference <= range;
		}

		public bool IsNearTo(Vector2Int posA, Vector2Int posB, int range)
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
			Vector2Int startPos = change.pos;
			Vector2Int direction = change.dir;
			Vector2Int newPos = startPos + direction;
			Vector2Int oppositeDirection = direction * -1;
			SetBlock(newPos, false);
			SetBlock(startPos, true);
			OnBlockMoved?.Invoke(newPos, oppositeDirection, 1f / 10f);
		}

		public void SetBlock(Vector2Int position, bool active)
		{
			int index = GetIndexFromPosition(position);
			if (index == -1) return;
			grid[index] = active;
		}

		public int GetIndexFromPosition(Vector2Int position)
		{
			if (IsOnOuterEdge(position)
				|| PositionOutOfBounds(position)) return -1;
			int index = position.y * GridSize.x + position.x;
			if (index <= 0 || index >= grid.Length) return -1;
			return index;
		}

		public Vector2Int GetPositionFromIndex(int index)
			=> new Vector2Int(index % GridSize.x, index / GridSize.x);

		private bool IsOnOuterEdge(Vector2Int position)
			=> IsNearOuterEdge(position, 0);

		public bool IsNearOuterEdge(Vector2Int position, int range)
		{
			int paddingRange = padding + range;
			return position.x < paddingRange
				|| position.y < paddingRange
				|| position.x >= GridSize.x - paddingRange
				|| position.y >= GridSize.y - paddingRange;
		}

		private bool PositionOutOfBounds(Vector2Int position)
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
			public Vector2Int pos, dir;

			public Change(Vector2Int pos, Vector2Int dir)
			{
				this.pos = pos;
				this.dir = dir;
			}
		}
	}
}
