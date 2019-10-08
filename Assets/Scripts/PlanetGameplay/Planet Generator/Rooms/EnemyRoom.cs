using System.Collections.Generic;
using UnityEngine;

public class EnemyRoom : Room
{
	float difficulty;

	public EnemyRoom(string[] lines, PlanetData data) : base(lines, data)
	{

	}

	public EnemyRoom(IntPair position, Room previousRoom, float difficulty)
		: base(position, previousRoom)
	{
		this.difficulty = difficulty;
	}

	public override void GenerateContent()
	{
		base.GenerateContent();

		List<RoomEnemy> enemies = EnemyRoomData.GenerateChallenge(difficulty, this);
		roomObjects.AddRange(enemies);

		for (int i = 0; i < enemies.Count; i++)
		{
			int xPos = Random.Range(3, RoomWidth - 3);
			int yPos = Random.Range(3, RoomHeight - 3);
			enemies[i].SetPosition(new IntPair(xPos, yPos));
		}
	}
}
