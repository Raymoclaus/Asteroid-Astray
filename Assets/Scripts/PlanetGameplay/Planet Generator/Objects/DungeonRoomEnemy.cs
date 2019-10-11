using UnityEngine;

public class DungeonRoomEnemy : DungeonRoomEntity
{
	public DungeonRoomEnemy(DungeonRoom currentRoom, Vector2 position,
		string objectName, object objectData, string enemyName)
		: base(currentRoom, position, objectName, objectData)
	{
		EnemyName = enemyName;
	}

	public string EnemyName { get; private set; }
}
