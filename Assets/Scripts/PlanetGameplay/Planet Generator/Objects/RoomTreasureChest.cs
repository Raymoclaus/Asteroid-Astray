public class RoomTreasureChest : RoomObject, ITextSaveLoader
{
	public bool IsOpen { get; set; }
	public bool IsLocked { get; set; }

	public RoomTreasureChest(Room room, bool locked) : base(room)
		=> IsLocked = locked;

	public RoomTreasureChest(Room room, string[] lines) : base(room, lines) { }

	public void Unlock()
	{
		IsLocked = false;
		IsOpen = true;
	}

	public override ObjType ObjectType => ObjType.TreasureChest;

	public override string ObjectName => "Treasure Chest";

	public const string SAVE_TAG = "[RoomTreasureChest]", SAVE_END_TAG = "[/RoomTreasureChest]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string GetSaveText(int indentLevel)
		=> $"{base.GetSaveText(indentLevel)}" +
		$"{new string('\t', indentLevel)}{isOpenProp}:{IsOpen}\n" +
		$"{new string('\t', indentLevel)}{isLockedProp}:{IsLocked}\n";

	private static readonly string isOpenProp = "isOpen";
	private static readonly string isLockedProp = "isLocked";
	public override void Load(string[] lines)
	{
		base.Load(lines);

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] props = line.Split(':');

			if (props[0] == isOpenProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				IsOpen = b;
				continue;
			}
			if (props[0] == isLockedProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				IsLocked = b;
				continue;
			}
		}
	}
}
