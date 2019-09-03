using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRoomTileLight : PlanetNonSolid
{
	[SerializeField] private SpriteRenderer sprRend;
	private RoomTileLight roomTileLight;

	public override void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(room, roomObject, dataSet);

		roomTileLight = (RoomTileLight)roomObject;
		roomTileLight.OnTileFlipped += Flip;
		Flip(roomTileLight.flipped);
	}

	private void OnDisable() => roomTileLight.OnTileFlipped -= Flip;

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		roomTileLight.Interact();
	}

	private void Flip(bool flip)
		=> sprRend.color = flip ? Color.blue : Color.white;
}
