using UnityEngine;

public class PlanetRoomTreasureChest : PlanetInteractable
{
	private RoomTreasureChest roomTreasureChest;
	[SerializeField] private Animator anim;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		roomTreasureChest = (RoomTreasureChest)roomObject;
		anim?.SetBool("Open", roomTreasureChest.IsOpen);
		EnableTrigger(!roomTreasureChest.IsOpen);
	}

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		Debug.Log("Unlock");
		Unlock();
	}

	private void Unlock()
	{
		roomTreasureChest.Unlock();
		anim?.SetBool("Open", roomTreasureChest.IsOpen);
		EnableTrigger(!roomTreasureChest.IsOpen);
	}
}
