public class StartRoom : DungeonRoom
{
	public StartRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		DungeonRoomObject landingPad = new DungeonRoomObject(this,
			CenterInt + IntPair.down * 3, "LandingPad", null);
		roomObjects.Add(landingPad);
	}
}
