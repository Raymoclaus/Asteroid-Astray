public class RoomDummy : RoomEntity
{
	public RoomDummy(Room room) : base(room) { }

	public RoomDummy(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomDummy]", SAVE_END_TAG = "[/RoomDummy]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override ObjType ObjectType => ObjType.Dummy;

	public override string ObjectName => "Dummy";
}
