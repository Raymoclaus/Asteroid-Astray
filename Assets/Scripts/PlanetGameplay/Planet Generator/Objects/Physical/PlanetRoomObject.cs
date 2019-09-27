using UnityEngine;

public abstract class PlanetRoomObject : MonoBehaviour
{
	protected Room room;
	protected RoomObject roomObject;
	protected bool hasBeenSetup;
	protected static int attackLayer = -1;
	protected static int AttackLayer => attackLayer == -1
		? (attackLayer = LayerMask.NameToLayer("Attack"))
		: attackLayer;

	public virtual void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		this.room = room;
		this.roomObject = roomObject;

		hasBeenSetup = true;
	}

	public Vector2Int GetPosition() => roomObject?.GetPosition() ?? Vector2Int.zero;

	public void SetRoomObjectWorldSpacePosition(Vector2Int position)
	{
		if (roomObject == null) return;

		Vector2Int roomSpacePos = position;
		if (room != null)
		{
			Vector3 roomWorldSpacePosition = room.GetWorldSpacePosition();
			Vector2Int roomWorldIntPosition = new Vector2Int(
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
