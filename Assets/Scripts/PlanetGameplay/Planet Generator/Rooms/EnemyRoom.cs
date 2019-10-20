using CustomDataTypes;
using System.Collections.Generic;

public class EnemyRoom : DungeonRoom
{
	float difficulty;

	public EnemyRoom(IntPair position, DungeonRoom previousRoom, float difficulty)
		: base(position, previousRoom)
	{
		this.difficulty = difficulty;
	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		List<DungeonRoomEnemy> enemies = EnemyRoomData.GenerateChallenge(difficulty, this);
		roomObjects.AddRange(enemies);
	}
}
