public class RoomGreenGroundButton : RoomGroundButton
{
	public RoomGreenGroundButton(Room room) : base(room) { }

	public RoomGreenGroundButton(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomGreenGroundButton]",
		SAVE_END_TAG = "[/RoomGreenGroundButton]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override ObjType ObjectType => ObjType.GreenGroundButton;

	public override string ObjectName => "Green Button";
}
