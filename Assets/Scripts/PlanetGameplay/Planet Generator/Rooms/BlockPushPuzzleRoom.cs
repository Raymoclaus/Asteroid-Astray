using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockPushPuzzle;

public class BlockPushPuzzleRoom : Room
{
	private float difficulty;
	private PushPuzzle puzzle;

	public BlockPushPuzzleRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public BlockPushPuzzleRoom(IntPair position, Room previousRoom)
		: base(position, previousRoom)
	{
		puzzle = CreateNewPushPuzzle();
		puzzle.OnPuzzleCompleted += UnlockAllExits;
	}

	public BlockPushPuzzleRoom(IntPair position, Room previousRoom, float difficulty)
		: this(position, previousRoom)
	{
		this.difficulty = difficulty;
	}

	private PushPuzzle CreateNewPushPuzzle() => new PushPuzzle(new IntPair(RoomWidth - 2, RoomHeight - 2), 1);

	public override void GenerateContent()
	{
		base.GenerateContent();

		BlockPushGenerator gen = new BlockPushGenerator();
		int padding = 1;
		int minimumSolutionCount = 1;
		puzzle = gen.Generate(puzzle.GridSize, padding, minimumSolutionCount);

		IntPair offset = IntPair.one;

		for (int i = 0; i < puzzle.grid.Length; i++)
		{
			IntPair position = puzzle.GetPositionFromIndex(i);
			bool isBlock = puzzle.BlockExists(position);
			if (!isBlock) continue;

			RoomPushableBlock roomBlock = new RoomPushableBlock(
				this, position + offset, puzzle, position);
			roomBlock.SetPosition(position + offset);
			roomObjects.Add(roomBlock);
		}
		RoomGreenGroundButton finishButton = new RoomGreenGroundButton(this);
		finishButton.SetPosition(puzzle.finishTile + offset);
		finishButton.OnButtonTriggered += puzzle.CompletePuzzle;
		roomObjects.Add(finishButton);

		for (int i = 0; i < puzzle.resetTiles.Length; i++)
		{
			RoomRedGroundButton resetButton = new RoomRedGroundButton(this);
			resetButton.SetPosition(puzzle.resetTiles[i] + offset);
			resetButton.SubscribeToHeldEvent(puzzle.RevertLastChange, 1f / 10f);
			roomObjects.Add(resetButton);
		}

		LockAllExceptPreviousRoom();
	}

	public override void Load(string[] lines)
	{
		base.Load(lines);

		puzzle = CreateNewPushPuzzle();

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			if (line == RoomPushableBlock.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomPushableBlock.SAVE_END_TAG);
				roomObjects.Add(new RoomPushableBlock(this, lines.SubArray(i, end), puzzle));
				i = end;
				continue;
			}
		}
	}
}
