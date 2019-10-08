[System.Serializable]
public class RoomSmokey : RoomEnemy
{
	public RoomSmokey(Room room) : base(room) { }

	public RoomSmokey(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomSmokey]",
		SAVE_END_TAG = "[/RoomSmokey]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string ObjectName => "Smokey";

	public override ObjType ObjectType => ObjType.Smokey;
}
