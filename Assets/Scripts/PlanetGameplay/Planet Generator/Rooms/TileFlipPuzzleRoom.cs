using CustomDataTypes;
using Puzzles.TileFlip;

public class TileFlipPuzzleRoom : DungeonRoom
{
	public GridMatrix puzzleGrid;
	private float puzzleDifficulty;

	public TileFlipPuzzleRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{
		puzzleGrid = CreateNewGridMatrix();
	}

	public TileFlipPuzzleRoom(IntPair position, DungeonRoom previousRoom, float difficulty)
		: this(position, previousRoom)
	{
		puzzleDifficulty = difficulty;
	}

	private GridMatrix CreateNewGridMatrix()
		=> new GridMatrix(new IntPair(RoomWidth / 2, RoomHeight / 2));

	public override void GenerateContent()
	{
		base.GenerateContent();

		Generator gen = new Generator();
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
