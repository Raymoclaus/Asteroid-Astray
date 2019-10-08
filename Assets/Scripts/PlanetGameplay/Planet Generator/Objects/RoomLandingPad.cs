[System.Serializable]
public class RoomLandingPad : RoomObject
{
	public RoomLandingPad(Room room) : base(room) { }

	public RoomLandingPad(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomLandingPad]",
		SAVE_END_TAG = "[/RoomLandingPad]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override ObjType ObjectType => ObjType.LandingPad;

	public override string ObjectName => "Leave Planet";
}
