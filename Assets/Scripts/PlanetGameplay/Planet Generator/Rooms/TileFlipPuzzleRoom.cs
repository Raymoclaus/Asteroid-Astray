using TileLightsPuzzle;

public class TileFlipPuzzleRoom : DungeonRoom
{
	public TileGrid puzzleGrid;
	private float puzzleDifficulty;

	public TileFlipPuzzleRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{
		puzzleGrid = CreateNewTileGrid();
	}

	public TileFlipPuzzleRoom(IntPair position, DungeonRoom previousRoom, float difficulty)
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
			DungeonRoomObject tileLight = new DungeonRoomObject(this,
				pos + offset, "FlipTile", i);
			roomObjects.Add(tileLight);
		}
	}
}
