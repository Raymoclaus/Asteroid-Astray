public class RoomTile : RoomObject, ITextSaveLoader
{
	public enum TileType
	{
		Floor,
		Wall
	}

	public override ObjType ObjectType => ObjType.Tile;

	public TileType type;

	public RoomTile(Room room, string[] lines) : base(room, lines) { }

	public RoomTile(Room room, IntPair position, TileType type) : base(room, position)
		=> this.type = type;

	public RoomTile(Room room, int x, int y, TileType type) : base(room, x, y)
		=> this.type = type;

	public const string SAVE_TAG = "[RoomTile]", SAVE_END_TAG = "[/RoomTile]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{tileTypeProp}:{type}\n";

	private static readonly string tileTypeProp = "tileType";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == tileTypeProp)
			{
				System.Enum.TryParse(props[1], out type);
				continue;
			}
		}
	}
}
