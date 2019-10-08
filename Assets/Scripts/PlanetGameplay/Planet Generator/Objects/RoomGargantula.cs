[System.Serializable]
public class RoomGargantula : RoomEnemy
{
	public RoomGargantula(Room room) : base(room) { }

	public RoomGargantula(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomGargantula]", SAVE_END_TAG = "[/RoomGargantula]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string ObjectName => "Gargantula";

	public override ObjType ObjectType => ObjType.Gargantula;
}
