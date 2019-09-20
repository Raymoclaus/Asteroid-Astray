using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileLightsPuzzle
{
	public class TileGrid
	{
		public Vector2Int GridSize { get; private set; }
		private bool[] grid;
		public List<Vector2Int> startingState = new List<Vector2Int>();

		public delegate void PuzzleCompletedEventHandler();
		public event PuzzleCompletedEventHandler OnPuzzleCompleted;

		public delegate void TileFlippedEventhandler(int index);
		public event TileFlippedEventhandler OnTileFlipped;

		public TileGrid(Vector2Int size)
		{
			GridSize = size;
			grid = new bool[GridSize.x * GridSize.y];
			SetupGrid();
		}

		private void SetupGrid()
		{
			for (int i = 0; i < grid.Length; i++)
			{
				grid[i] = true;
			}
		}

		public void Reset()
		{
			SetupGrid();
			for (int i = 0; i < startingState.Count; i++)
			{
				TileFlipped(startingState[i]);
			}
		}

		public void TileFlipped(int index) => TileFlipped(GetPosition(index));
		public void TileFlipped(Vector2Int position)
		{
			if (GetIndex(position) == -1) return;

			//center
			FlipTile(position);
			//up
			FlipTile(new Vector2Int(position.x, position.y + 1));
			//right
			FlipTile(new Vector2Int(position.x + 1, position.y));
			//down
			FlipTile(new Vector2Int(position.x, position.y - 1));
			//left
			FlipTile(new Vector2Int(position.x - 1, position.y));

			if (PuzzleIsCompleted())
			{
				OnPuzzleCompleted?.Invoke();
				Debug.Log("Tile Lights Puzzle Completed");
			}
		}

		private bool PuzzleIsCompleted()
		{
			for (int i = 0; i < grid.Length; i++)
			{
				if (!grid[i]) return false;
			}
			return true;
		}

		private void FlipTile(Vector2Int position)
		{
			int index = GetIndex(position);
			if (index == -1) return;
			grid[index] = !grid[index];
			OnTileFlipped?.Invoke(index);
		}

		public bool StartingStateContainsPosition(Vector2Int position)
		{
			for (int i = 0; i < startingState.Count; i++)
			{
				if (startingState[i] == position) return true;
			}
			return false;
		}

		private int GetIndex(Vector2Int position)
		{
			if (position.x < 0
				|| position.y < 0
				|| position.x >= GridSize.x
				|| position.y >= GridSize.y) return -1;
			return position.y * GridSize.x + position.x;
		}

		public Vector2Int GetPosition(int index)
			=> new Vector2Int(index % GridSize.x, index / GridSize.x);

		public bool IsFlipped(int index) => grid[index];

		public int GetArrayLength() => grid.Length;
	}
}
