using UnityEngine;
using System;

[System.Serializable]
public class SpawnableEntity
{
	//reference to the prefab
	public Entity prefab;
	//name of the Entity type
	public string name = "default";
	//chance that the chunk will be filled with this entity type
	[Range(0.0001f, 1f)]
	public float rarity;
	//entities with a rarity offset won't appear before a specified zone
	public int rarityZoneOffset = 1;
	//entities with a rarity cutoff will not appear after a specified zone (numbers lower than 0 for no cutoff)
	public int rarityZoneCutoff = -1;
	//rarity increases from 0 until given rarity. Minimum value is 0. Higher values means less steep curve
	public float rarityIncreaseSteepness = 1f;
	//in the event this entity type is spawned, how many should spawn in the chunk
	public IntPair spawnRange;
	//determines where in a chunk the entity should be spawned
	public SpawnPosition posType = SpawnPosition.Random;
	//convenient off switch for telling the entity generator to ignore this
	public bool ignore = false;
	//should not spawn in the same chunk as other entities
	public bool spacePriority = false;

	public enum SpawnPosition
	{
		Random,
		Center
	}

	public float GetChance(float distance)
	{
		if (rarity <= 0f) return 0f;

		int zone = Difficulty.DistanceBasedDifficulty(distance);
		if (zone < rarityZoneOffset) return 0f;
		if (rarityZoneCutoff >= 0 && zone > rarityZoneOffset + rarityZoneCutoff) return 0f;

		float a = Mathf.Clamp01(rarity);
		float b = Mathf.Max(0, rarityIncreaseSteepness);
		float c = Math.Max(0, rarityZoneOffset);
		float x = distance / Constants.CHUNK_SIZE;

		return a * ((x - c)/(x - c + b));
	}

	public float GetMinimumDistanceToBeSpawned()
	{
		float dist = 0f;
		float chance = 0f;
		while (chance <= 0f)
		{
			chance = GetChance(dist);
			dist += Constants.CHUNK_SIZE;
		}
		return dist;
	}
}