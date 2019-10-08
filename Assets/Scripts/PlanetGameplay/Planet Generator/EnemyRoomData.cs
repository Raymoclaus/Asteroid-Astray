using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyRoomData
{
	private static List<EnemyDifficulty> enemyDifficulties = new List<EnemyDifficulty>();
	private static float lowestDifficultyLevel = Mathf.Infinity;

	public static List<RoomEnemy> GenerateChallenge(float difficulty, Room room)
	{
		List<RoomEnemy> enemies = new List<RoomEnemy>();
		GetEnemyDifficultiesFromProject();
		
		while (difficulty >= lowestDifficultyLevel)
		{
			EnemyDifficulty? pedmNullable = PickRandomEnemy();
			if (pedmNullable == null) continue;
			EnemyDifficulty pedm = (EnemyDifficulty)pedmNullable;
			if (pedm.difficultyLevel <= difficulty)
			{
				difficulty -= pedm.difficultyLevel;
				enemies.Add(CreateEnemy(pedm.enemyType, room));
			}
		}

		return enemies;
	}

	private static RoomEnemy CreateEnemy(Type enemyType, Room room)
		=> (RoomEnemy)Activator.CreateInstance(enemyType, room);

	private static EnemyDifficulty? PickRandomEnemy()
	{
		if (enemyDifficulties.Count == 0) return null;
		int choose = (int)(UnityEngine.Random.value * enemyDifficulties.Count);
		int count = 0;
		foreach (EnemyDifficulty pedm in enemyDifficulties)
		{
			if (count == choose) return pedm;
			count++;
		}
		return null;
	}

	private static void GetEnemyDifficultiesFromProject()
	{
		Type baseType = typeof(RoomEnemy);
		IEnumerable<Type> enumerable = Assembly.GetAssembly(baseType).GetTypes()
			.Where(t => t.IsSubclassOf(baseType) && t != baseType);

		foreach (Type enemyType in enumerable)
		{
			AddToEnemyDifficulties(enemyType);
		}
	}

	private static void AddToEnemyDifficulties(Type enemyType)
	{
		if (EnemyDifficultiesContainsType(enemyType)) return;
		
		Type t = enemyType;
		FieldInfo field = null;
		do
		{
			field = t.GetField("DIFFICULTY_LEVEL");
		} while (field == null && (t = t.BaseType) != null);

		if (field == null)
		{
			Debug.Log("No field found");
			return;
		}

		object difficultyObj = field.GetRawConstantValue();
		float difficulty = (float)difficultyObj;
		if (difficulty == Mathf.Infinity) return;
		EnemyDifficulty eDiff = new EnemyDifficulty(enemyType, difficulty);
		enemyDifficulties.Add(eDiff);
		lowestDifficultyLevel = Mathf.Min(lowestDifficultyLevel, eDiff.difficultyLevel);
	}

	private static bool EnemyDifficultiesContainsType(Type enemyType)
	{
		for (int i = 0; i < enemyDifficulties.Count; i++)
		{
			if (enemyDifficulties[i].enemyType == enemyType) return true;
		}
		return false;
	}

	public struct EnemyDifficulty
	{
		public Type enemyType;
		public float difficultyLevel;

		public EnemyDifficulty(Type enemyType, float difficultyLevel)
		{
			this.enemyType = enemyType;
			this.difficultyLevel = difficultyLevel;
		}
	}
}
