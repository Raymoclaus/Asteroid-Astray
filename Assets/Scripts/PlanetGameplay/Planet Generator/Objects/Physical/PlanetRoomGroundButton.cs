public class PlanetRoomGroundButton : PlanetVicinityTrigger
{
	private RoomGroundButton roomGroundBtn;

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject,
		PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);
		roomGroundBtn = (RoomGroundButton)roomObject;
	}

	protected override void PlanetActorTriggered(PlanetTriggerer actor)
	{
		base.PlanetActorTriggered(actor);
		roomGroundBtn.Trigger(this);
	}

	protected override void PlanetActorUntriggered(PlanetTriggerer actor)
	{
		base.PlanetActorUntriggered(actor);
		if (nearbyActors.Count > 0) return;
		roomGroundBtn.Release();
	}
}
