[System.Serializable]
public class RoomBoomee : RoomEnemy
{
	public RoomBoomee(Room room) : base(room) { }

	public RoomBoomee(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomBoomee]", SAVE_END_TAG = "[/RoomBoomee]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string ObjectName => "Boomee";

	public override ObjType ObjectType => ObjType.Boomee;
}
