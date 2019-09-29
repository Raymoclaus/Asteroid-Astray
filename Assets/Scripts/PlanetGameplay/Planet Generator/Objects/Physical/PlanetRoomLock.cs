using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRoomLock : PlanetInteractable
{
	[SerializeField] private SpriteRenderer sprRend;
	public RoomKey.KeyColour colour;
	public Direction direction;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		RoomLock lockObj = (RoomLock)roomObject;
		colour = lockObj.colour;
		sprRend.sprite = dataSet.locks[(int)colour];
		direction = ((RoomLock)roomObject).direction;
	}

	public void Unlock()
	{
		room.AttemptUnlock(direction);
		Destroy(gameObject);
	}
}
