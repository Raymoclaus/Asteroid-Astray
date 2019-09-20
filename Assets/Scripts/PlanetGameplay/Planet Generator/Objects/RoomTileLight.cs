using TileLightsPuzzle;

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

	public RoomTileLight(TileGrid tileGrid, int index)
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

	public override ObjType GetObjectType() => ObjType.TileLight;
}
