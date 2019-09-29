using UnityEngine;

public abstract class PlanetRoomObject : MonoBehaviour
{
	protected RoomViewer roomViewer;
	protected Room room;
	protected RoomObject roomObject;
	protected bool hasBeenSetup;
	protected static int attackLayer = -1;
	protected static int AttackLayer => attackLayer == -1
		? (attackLayer = LayerMask.NameToLayer("Attack"))
		: attackLayer;

	public virtual void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		this.roomViewer = roomViewer;
		this.room = room;
		this.roomObject = roomObject;

		hasBeenSetup = true;
	}

	public IntPair GetPosition() => roomObject?.GetPosition() ?? IntPair.zero;

	public void SetRoomObjectWorldSpacePosition(IntPair position)
	{
		if (roomObject == null) return;

		IntPair roomSpacePos = position;
		if (room != null)
		{
			Vector3 roomWorldSpacePosition = room.GetWorldSpacePosition();
			IntPair roomWorldIntPosition = new IntPair(
				(int)roomWorldSpacePosition.x, (int)roomWorldSpacePosition.y);
			roomSpacePos -= roomWorldIntPosition;
		}
		else
		{
			Debug.Log("Room is null", this);
		}
		if (roomSpacePos == GetPosition()) return;
		roomObject.SetPosition(roomSpacePos);
	}

	protected void SetRoom(Room room) => this.room = room;

	public Room Room => room;
}
