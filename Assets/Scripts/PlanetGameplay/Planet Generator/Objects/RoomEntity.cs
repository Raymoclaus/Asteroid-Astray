[System.Serializable]
public abstract class RoomEntity : RoomObject
{
	public RoomEntity(Room room) : base(room) { }

	public RoomEntity(Room room, string[] lines) : base(room, lines) { }
}
