public abstract class RoomObject : ITextSaveLoader
{
	private IntPair position;
	public Room CurrentRoom { get; private set; }

	public delegate void PositionUpdatedEventHandler(IntPair position);
	public event PositionUpdatedEventHandler OnPositionUpdated;

	public RoomObject(Room room) => CurrentRoom = room;

	public RoomObject(Room room, string[] lines) : this(room)
	{
		Load(lines);
	}

	public RoomObject(Room room, IntPair position) : this(room)
	{
		this.position = position;
	}

	public RoomObject(Room room, int x, int y) : this(room, new IntPair(x, y)) { }

	public IntPair Position => position;
	public void SetPosition(IntPair position)
	{
		if (this.position == position) return;
		this.position = position;
		OnPositionUpdated?.Invoke(position);
	}

	public virtual ObjType ObjectType => ObjType.None;

	public virtual string ObjectName => "Object";

	public enum ObjType
	{
		None,
		Lock,
		Key,
		ExitTrigger,
		LandingPad,
		TileLight,
		PushableBlock,
		Player,
		GreenGroundButton,
		RedGroundButton,
		Dummy,
		Spooder,
		Charger,
		Boomee,
		Smokey,
		Gargantula,
		TreasureChest,
		Tile
	}

	public virtual string Tag => string.Empty;

	public virtual string EndTag => string.Empty;

	public virtual void PrepareForSaving() { }

	public virtual void FinishedLoading() { }

	public virtual string GetSaveText(int indentLevel)
		=> $"{new string('\t', indentLevel)}{positionProp}:{position}\n";

	public virtual ITextSaveLoader[] GetObjectsToSave() => null;

	private string positionProp = "position";
	public virtual void Load(string[] lines)
	{
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == positionProp)
			{
				IntPair.TryParse(props[1], out position);
				continue;
			}
		}
	}
}
