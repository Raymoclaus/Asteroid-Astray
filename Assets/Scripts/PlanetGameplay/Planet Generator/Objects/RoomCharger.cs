[System.Serializable]
public class RoomCharger : RoomEnemy
{
	public RoomCharger(Room room) : base(room) { }

	public RoomCharger(Room room, string[] lines) : base(room, lines) { }

	public const string SAVE_TAG = "[RoomCharger]", SAVE_END_TAG = "[/RoomCharger]";
	public override string Tag => SAVE_TAG;
	public override string EndTag => SAVE_END_TAG;

	public override string ObjectName => "Charger";
}
