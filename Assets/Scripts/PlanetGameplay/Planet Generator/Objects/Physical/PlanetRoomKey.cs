using UnityEngine;

public class PlanetRoomKey : PlanetNonSolid
{
	[SerializeField] private SpriteRenderer sprRend;
	private RoomKey.KeyColour colour;
	private RoomKey key;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);
		
		key = (RoomKey)roomObject;
		if (key.hidden)
		{
			HideKey();
			key.OnKeyRevealed += RevealKey;
		}
		colour = key.colour;
		sprRend.sprite = dataSet.keys[(int)colour];
	}

	private void OnDisable() => key.OnKeyRevealed -= RevealKey;

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		Pickup();
	}

	private void HideKey()
	{
		sprRend.enabled = false;
		EnableTrigger(false);
	}

	private void RevealKey()
	{
		sprRend.enabled = true;
		EnableTrigger(true);
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
