using TileLightsPuzzle;

[System.Serializable]
public class RoomTileLight : RoomObject
{
	public bool flipped;
	private TileGrid tileGrid;
	private int index;
	public bool puzzleCompleted;

	public delegate void TileFlippedEventHandler(bool flipped);
	public event TileFlippedEventHandler OnTileFlipped;
	public delegate void PuzzleCompletedEventHandler();
	public event PuzzleCompletedEventHandler OnPuzzleCompleted;

	public RoomTileLight(Room room, TileGrid tileGrid, string[] lines) : base(room)
	{
		this.tileGrid = tileGrid;
		tileGrid.SetFlipped(index, flipped);
	}

	public RoomTileLight(Room room, TileGrid tileGrid, int index) : base(room)
	{
		this.tileGrid = tileGrid;
		this.index = index;
		if (tileGrid != null)
		{
			tileGrid.OnTileFlipped += Flip;
			tileGrid.OnPuzzleCompleted += CompletePuzzle;
			flipped = tileGrid.IsFlipped(index);
		}
	}
	
	private void Flip(int index)
	{
		if (this.index != index) return;
		flipped = !flipped;
		OnTileFlipped?.Invoke(flipped);
	}

	private void CompletePuzzle()
	{
		puzzleCompleted = true;
		OnPuzzleCompleted?.Invoke();
	}

	public void Interact() => tileGrid.TileFlipped(index);

	public override ObjType ObjectType => ObjType.TileLight;

	public override string ObjectName => "Flip Puzzle Tile";

	public const string SAVE_TAG = "[RoomTileLight]", SAVE_END_TAG = "[/RoomTileLight]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{flippedProp}:{flipped}\n" +
		$"{new string('\t', indentLevel)}{indexProp}:{index}\n" +
		$"{new string('\t', indentLevel)}{puzzleCompletedProp}:{puzzleCompleted}\n";

	private static readonly string flippedProp = "flipped";
	private static readonly string indexProp = "index";
	private static readonly string puzzleCompletedProp = "puzzleCompleted";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == flippedProp)
			{
				bool.TryParse(props[1], out flipped);
				continue;
			}
			if (props[0] == indexProp)
			{
				int.TryParse(props[1], out index);
				continue;
			}
			if (props[0] == puzzleCompletedProp)
			{
				bool.TryParse(props[1], out puzzleCompleted);
				continue;
			}
		}
	}
}
