using TileLightsPuzzle;

public class TileFlipPuzzleRoom : Room
{
	public TileGrid puzzleGrid;
	private float puzzleDifficulty;

	public TileFlipPuzzleRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public TileFlipPuzzleRoom(IntPair position, Room previousRoom)
		: base(position, previousRoom)
	{
		puzzleGrid = CreateNewTileGrid();
	}

	public TileFlipPuzzleRoom(IntPair position, Room previousRoom, float difficulty)
		: this(position, previousRoom)
	{
		puzzleDifficulty = difficulty;
	}

	private TileGrid CreateNewTileGrid()
		=> new TileGrid(new IntPair(RoomWidth / 2, RoomHeight / 2));

	public override void GenerateContent()
	{
		base.GenerateContent();

		TileLightsGenerator gen = new TileLightsGenerator();
		puzzleGrid = gen.GeneratePuzzle(puzzleGrid.GridSize, (int)puzzleDifficulty);

		IntPair offset = new IntPair(RoomWidth / 2 - puzzleGrid.GridSize.x / 2,
			RoomHeight / 2 - puzzleGrid.GridSize.y / 2);
		for (int i = 0; i < puzzleGrid.GetArrayLength(); i++)
		{
			IntPair pos = puzzleGrid.GetPosition(i);

			bool flipped = puzzleGrid.IsFlipped(i);
			RoomTileLight tileLight = new RoomTileLight(this, puzzleGrid, i);
			tileLight.SetPosition(pos + offset);
			roomObjects.Add(tileLight);
		}
	}

	public override void Load(string[] lines)
	{
		base.Load(lines);

		puzzleGrid = CreateNewTileGrid();

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			if (line == RoomTileLight.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomTileLight.SAVE_END_TAG);
				roomObjects.Add(new RoomTileLight(this, puzzleGrid, lines.SubArray(i, end)));
				i = end;
				continue;
			}
		}
	}
}
