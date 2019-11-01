using UnityEngine;

public class DungeonRoomEnemy : DungeonRoomEntity
{
	public DungeonRoomEnemy(DungeonRoom currentRoom, Vector2 position,
		string objectName, object objectData, bool isPersistent, string enemyName)
		: base(currentRoom, position, objectName, objectData, isPersistent)
	{
		EnemyName = enemyName;
	}

	public string EnemyName { get; private set; }
}
