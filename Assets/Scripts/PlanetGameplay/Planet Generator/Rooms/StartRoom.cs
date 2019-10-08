public class StartRoom : Room
{
	public StartRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public StartRoom(IntPair position, Room previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		RoomLandingPad landingPad = new RoomLandingPad(this);
		landingPad.SetPosition(CenterInt + IntPair.down * 3);
		roomObjects.Add(landingPad);
	}
}
