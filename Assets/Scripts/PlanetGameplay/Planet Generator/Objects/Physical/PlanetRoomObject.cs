using UnityEngine;

public abstract class PlanetRoomObject : MonoBehaviour
{
	protected Room room;
	protected RoomObject roomObject;
	protected bool hasBeenSetup;

	public virtual void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		this.room = room;
		this.roomObject = roomObject;

		hasBeenSetup = true;
	}
}
