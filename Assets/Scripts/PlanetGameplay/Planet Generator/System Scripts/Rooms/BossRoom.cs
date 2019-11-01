using CustomDataTypes;
using System.Collections.Generic;

public class BossRoom : DungeonRoom
{
	float difficulty;

	public BossRoom(IntPair position, DungeonRoom previousRoom, float difficulty)
		: base(position, previousRoom)
	{
		this.difficulty = difficulty;
	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		List<DungeonRoomEnemy> enemies = EnemyRoomData.GenerateChallenge(difficulty, this);
		roomObjects.AddRange(enemies);

		IntPair pos = CenterInt * IntPair.right + InnerDimensions * IntPair.up;
		DungeonRoomObject treasureChest = new DungeonRoomObject(this, pos,
			"LockedTreasureChest", null, false);
		roomObjects.Add(treasureChest);
	}
}
