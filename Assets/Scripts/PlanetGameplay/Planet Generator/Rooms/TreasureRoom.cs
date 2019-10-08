public class TreasureRoom : Room
{
	public TreasureRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public TreasureRoom(IntPair position, Room previousRoom) : base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		IntPair pos = CenterInt + IntPair.up;
		RoomTreasureChest treasureChest = new RoomTreasureChest(this, false);
		treasureChest.SetPosition(pos);
		roomObjects.Add(treasureChest);
	}
}
