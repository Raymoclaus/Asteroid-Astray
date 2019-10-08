[System.Serializable]
public class RoomPlayer : RoomEntity
{
	public RoomPlayer(Room room) : base(room) { }

	public RoomPlayer(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomPlayer]",
		SAVE_END_TAG = "[/RoomPlayer]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override ObjType ObjectType => ObjType.Player;

	public override string ObjectName => "Player";
}
