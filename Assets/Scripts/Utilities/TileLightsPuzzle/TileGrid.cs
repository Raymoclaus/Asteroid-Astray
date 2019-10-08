using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileLightsPuzzle
{
	public class TileGrid
	{
		public IntPair GridSize { get; private set; }
		private bool[] grid;
		public List<IntPair> startingState = new List<IntPair>();

		public delegate void PuzzleCompletedEventHandler();
		public event PuzzleCompletedEventHandler OnPuzzleCompleted;

		public delegate void TileFlippedEventhandler(int index);
		public event TileFlippedEventhandler OnTileFlipped;

		public TileGrid(IntPair size)
		{
			GridSize = size;
			grid = new bool[GridSize.x * GridSize.y];
			SetupGrid();
		}

		private void SetupGrid()
		{
			for (int i = 0; i < GetArrayLength(); i++)
			{
				SetFlipped(i, true);
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
		public void TileFlipped(IntPair position)
		{
			if (GetIndex(position) == -1) return;

			//center
			FlipTile(position);
			//up
			FlipTile(new IntPair(position.x, position.y + 1));
			//right
			FlipTile(new IntPair(position.x + 1, position.y));
			//down
			FlipTile(new IntPair(position.x, position.y - 1));
			//left
			FlipTile(new IntPair(position.x - 1, position.y));

			if (PuzzleIsCompleted())
			{
				OnPuzzleCompleted?.Invoke();
				Debug.Log("Tile Lights Puzzle Completed");
			}
		}

		private bool PuzzleIsCompleted()
		{
			for (int i = 0; i < GetArrayLength(); i++)
			{
				if (!IsFlipped(i)) return false;
			}
			return true;
		}

		private void FlipTile(IntPair position)
		{
			int index = GetIndex(position);
			if (index == -1) return;
			SetFlipped(index, !IsFlipped(index));
			OnTileFlipped?.Invoke(index);
		}

		public bool StartingStateContainsPosition(IntPair position)
		{
			for (int i = 0; i < startingState.Count; i++)
			{
				if (startingState[i] == position) return true;
			}
			return false;
		}

		private int GetIndex(IntPair position)
		{
			if (position.x < 0
				|| position.y < 0
				|| position.x >= GridSize.x
				|| position.y >= GridSize.y) return -1;
			return position.y * GridSize.x + position.x;
		}

		public IntPair GetPosition(int index)
			=> new IntPair(index % GridSize.x, index / GridSize.x);

		public bool IsFlipped(int index) => grid[index];

		public int GetArrayLength() => grid.Length;

		public void SetFlipped(int index, bool flipped) => grid[index] = flipped;
	}
}
