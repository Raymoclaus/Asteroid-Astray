[System.Serializable]
public class RoomSpooder : RoomEnemy
{
	public new const float DIFFICULTY_LEVEL = 1f;

	public RoomSpooder(Room room) : base(room) { }

	public RoomSpooder(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomSpooder]",
		SAVE_END_TAG = "[/RoomSpooder]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override float DifficultyModifier => DIFFICULTY_LEVEL;

	public override string ObjectName => "Spooder";

	public override ObjType ObjectType => ObjType.Spooder;
}
