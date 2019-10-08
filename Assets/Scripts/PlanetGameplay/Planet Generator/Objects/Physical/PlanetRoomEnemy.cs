using UnityEngine;

public abstract class PlanetRoomEnemy : PlanetRoomEntity
{
	protected PlanetPlayer player;
	protected float DistanceToPlayer
		=> player != null ?
		Vector3.Distance(GetPivotPosition(), player.GetPivotPosition())
		: Mathf.Infinity;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		roomViewer.OnRoomChanged += RoomChanged;
		FindPlayer();
	}

	private void RoomChanged(Room newRoom, Direction direction) => FindPlayer();

	protected virtual void FindPlayer()
		=> player = player ?? FindObjectOfType<PlanetPlayer>();

	protected bool PlayerInRoom => player != null && player.Room == room;
}
