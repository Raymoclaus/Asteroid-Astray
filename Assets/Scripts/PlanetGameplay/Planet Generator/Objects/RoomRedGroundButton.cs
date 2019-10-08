public class RoomRedGroundButton : RoomGroundButton
{
	public RoomRedGroundButton(Room room) : base(room) { }

	public RoomRedGroundButton(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomRedGroundButton]",
		SAVE_END_TAG = "[/RoomRedGroundButton]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override ObjType ObjectType => ObjType.RedGroundButton;
}
