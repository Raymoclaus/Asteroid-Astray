using CustomDataTypes;

public class StartRoom : DungeonRoom
{
	public StartRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();


		IntPair landingPadPos = CenterInt + IntPair.down * 3;
		DungeonRoomObject landingPad = new DungeonRoomObject(this,
			landingPadPos, "LandingPad", null, false);
		roomObjects.Add(landingPad);

		IntPair playerPos = landingPadPos + IntPair.down * 2;
		DungeonRoomObject player = new DungeonRoomObject(this,
			playerPos, "PlanetPlayer", null, true);
		roomObjects.Add(player);
	}
}
