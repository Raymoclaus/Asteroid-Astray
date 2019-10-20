using CustomDataTypes;

public class TreasureRoom : DungeonRoom
{
	public TreasureRoom(IntPair position, DungeonRoom previousRoom)
		: base(position, previousRoom)
	{

	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		IntPair pos = CenterInt + IntPair.up;
		DungeonRoomObject treasureChest = new DungeonRoomObject(this, pos,
			"TreasureChest", null);
		roomObjects.Add(treasureChest);
	}
}
