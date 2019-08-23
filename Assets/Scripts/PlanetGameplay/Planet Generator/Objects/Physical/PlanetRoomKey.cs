using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRoomKey : PlanetNonSolid
{
	[SerializeField] private SpriteRenderer sprRend;
	private RoomKey.KeyColour colour;

	public override void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(room, roomObject, dataSet);

		RoomKey key = (RoomKey)roomObject;
		colour = key.colour;
		sprRend.sprite = dataSet.keys[(int)colour];
	}

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		Pickup();
	}

	public void Pickup()
	{
		room.RemoveObject(roomObject);
		Destroy(gameObject);
	}

	public ItemStack GetItem()
	{
		switch (colour)
		{
			default: return new ItemStack(Item.Type.BlueKey, 1);
			case RoomKey.KeyColour.Blue: return new ItemStack(Item.Type.BlueKey, 1);
			case RoomKey.KeyColour.Red: return new ItemStack(Item.Type.RedKey, 1);
			case RoomKey.KeyColour.Yellow: return new ItemStack(Item.Type.YellowKey, 1);
			case RoomKey.KeyColour.Green: return new ItemStack(Item.Type.GreenKey, 1);
		}
	}
}
