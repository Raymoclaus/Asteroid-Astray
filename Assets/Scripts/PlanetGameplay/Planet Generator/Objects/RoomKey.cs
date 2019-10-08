using System.Collections.Generic;
using TileLightsPuzzle;
using BlockPushPuzzle;

public class RoomKey : RoomObject
{
	public enum KeyColour
	{
		Blue,
		Red,
		Yellow,
		Green
	}

	public KeyColour colour;
	public bool hidden;

	public delegate void RevealedEventHandler();
	public event RevealedEventHandler OnKeyRevealed;

	public RoomKey(Room room, string[] lines) : base(room, lines) { }

	public RoomKey(Room room, KeyColour colour) : base(room)
	{
		this.colour = colour;
	}

	private void FindNewPlaceInMaze(MazePuzzle.Maze maze)
	{
		List<IntPair> path = maze.GetLongestPath();
		SetPosition(path[path.Count - 1]);
	}

	private void FindNewPlaceForTileLightsPuzzle(TileGrid tileGrid)
	{
		IntPair roomCenter = CurrentRoom.CenterInt;
		int puzzleHeight = tileGrid.GridSize.y;
		SetPosition(new IntPair(roomCenter.x, roomCenter.y + puzzleHeight / 2 + 1));
		hidden = true;
		tileGrid.OnPuzzleCompleted += RevealKey;
	}

	private void FindNewPlaceForBlockPushPuzzle(PushPuzzle puzzle)
	{
		IntPair roomCenter = CurrentRoom.CenterInt;
		int puzzleHeight = puzzle.GridSize.y;
		SetPosition(new IntPair(roomCenter.x, roomCenter.y + puzzleHeight / 2 - 1));
		hidden = true;
		puzzle.OnPuzzleCompleted += RevealKey;
	}

	private void RevealKey()
	{
		hidden = false;
		OnKeyRevealed?.Invoke();
	}

	public override ObjType ObjectType => ObjType.Key;

	public static Item.Type ConvertToItemType(KeyColour col)
	{
		switch (col)
		{
			default: return Item.Type.BlueKey;
			case KeyColour.Blue: return Item.Type.BlueKey;
			case KeyColour.Red: return Item.Type.RedKey;
			case KeyColour.Yellow: return Item.Type.YellowKey;
			case KeyColour.Green: return Item.Type.GreenKey;
		}
	}

	public override string ObjectName => $"{colour} Key";

	public const string SAVE_TAG = "[RoomKey]", SAVE_END_TAG = "[/RoomKey]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{colourProp}:{colour}\n" +
		$"{new string('\t', indentLevel)}{hiddenProp}:{hidden}\n";

	private static readonly string colourProp = "colour";
	private static readonly string hiddenProp = "hidden";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == colourProp)
			{
				System.Enum.TryParse(props[1], out colour);
				continue;
			}
			if (props[0] == hiddenProp)
			{
				bool.TryParse(props[1], out hidden);
				continue;
			}
		}
	}
}
