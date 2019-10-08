using UnityEngine;

public class PlanetRoomLock : PlanetInteractable
{
	[SerializeField] private SpriteRenderer sprRend;
	public RoomKey.KeyColour colour;
	public Direction direction;
	private RoomLock roomLock;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		roomLock = (RoomLock)roomObject;
		colour = roomLock.colour;
		sprRend.sprite = dataSet.locks[(int)colour];
		direction = roomLock.direction;

		roomLock.CurrentRoom.OnExitUnlocked += Unlock;
	}

	private void OnDisable()
	{
		roomLock.CurrentRoom.OnExitUnlocked -= Unlock;
	}

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		AttemptUnlock(actor);
	}

	private void AttemptUnlock(Triggerer actor) => roomLock.AttemptUnlock(actor);


	public void Unlock(Direction direction)
	{
		if (this.direction != direction) return;
		Destroy(gameObject);
	}
}
