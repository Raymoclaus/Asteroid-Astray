using UnityEngine;

public class PlanetRoomTileLight : PlanetNonSolid
{
	[SerializeField] private SpriteRenderer sprRend;
	private RoomTileLight roomTileLight;

	public override void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(room, roomObject, dataSet);

		roomTileLight = (RoomTileLight)roomObject;
		if (roomTileLight.puzzleCompleted)
		{
			RemoveInteraction();
		}
		else
		{
			roomTileLight.OnTileFlipped += Flip;
			roomTileLight.OnPuzzleCompleted += RemoveInteraction;
		}
		Flip(roomTileLight.flipped);
	}

	private void RemoveInteraction() => EnableTrigger(false);

	private void OnDisable()
	{
		roomTileLight.OnTileFlipped -= Flip;
		roomTileLight.OnPuzzleCompleted -= RemoveInteraction;
	}

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		roomTileLight.Interact();
	}

	private void Flip(bool flip)
		=> sprRend.color = flip ? Color.blue : Color.white;
}
