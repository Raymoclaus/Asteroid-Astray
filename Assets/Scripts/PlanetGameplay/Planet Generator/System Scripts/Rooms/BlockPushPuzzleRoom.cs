using System;
using UnityEngine;
using Puzzles.BlockPush;
using CustomDataTypes;

public class BlockPushPuzzleRoom : DungeonRoom
{
	private float difficulty;
	private GridMatrix puzzle;

	public BlockPushPuzzleRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{
	}

	public BlockPushPuzzleRoom(IntPair position, DungeonRoom previousRoom, float difficulty)
		: this(position, previousRoom)
	{
		this.difficulty = difficulty;
	}

	private IntPair PuzzleSize
		=> new IntPair(RoomWidth / ExitWidth - 1, RoomHeight / ExitWidth - 1);

	public override void GenerateContent()
	{
		base.GenerateContent();

		Generator gen = new Generator();
		int padding = 1;
		int minimumSolutionCount = 1;
		puzzle = gen.Generate(PuzzleSize, padding, minimumSolutionCount);
		puzzle.OnPuzzleCompleted += UnlockAllExitsOfLockTypeNone;
		Debug.Log(puzzle.GridSize);

		IntPair offset = IntPair.one;
		int tileSize = ExitWidth;

		for (int i = 0; i < puzzle.grid.Length; i++)
		{
			IntPair puzzlePos = puzzle.GetPositionFromIndex(i);
			bool isBlock = puzzle.BlockExists(puzzlePos);
			if (!isBlock) continue;

			DungeonRoomObject roomBlock = new DungeonRoomObject(
				this, puzzlePos * tileSize + offset, "PushableBlock",
				puzzlePos, false);
			roomObjects.Add(roomBlock);
		}
		DungeonRoomObject finishButton = new DungeonRoomObject(this,
			puzzle.finishTile * tileSize + offset, "GreenButton",
			(Action)puzzle.CompletePuzzle, false);
		roomObjects.Add(finishButton);

		for (int i = 0; i < puzzle.resetTiles.Length; i++)
		{
			DungeonRoomObject resetButton = new DungeonRoomObject(this,
				puzzle.resetTiles[i] * tileSize + offset, "RedButton",
				(Action)puzzle.RevertLastChange, false);
			roomObjects.Add(resetButton);
		}

		LockAllExceptPreviousRoom();
	}
}
