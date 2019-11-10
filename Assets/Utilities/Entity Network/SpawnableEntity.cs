using UnityEngine;
using System;
using CustomDataTypes;

[CreateAssetMenu(menuName = "Scriptable Objects/Spawnable Entity")]
public class SpawnableEntity : ScriptableObject
{
	//reference to the prefab
	public Entity prefab;
	//name of the Entity type
	public string entityName = "default";
	//chance that the chunk will be filled with this entity type compared to others
	public float startingRarity, endingRarity;
	//entities with a rarity offset won't appear before a specified zone
	public int rarityZoneOffset = 1;
	//Number of zones the entity can spawn in after the zone offset. Use 0 or lower for infinite spawn range
	public int rarityZoneCount = -1;
	//in the event this entity type is spawned, how many should spawn in the chunk
	public int minSpawnCountInChunk, maxSpawnCountInChunk;
	//determines where in a chunk the entity should be spawned
	public SpawnPosition posType = SpawnPosition.Random;
	//convenient off switch for telling the entity generator to ignore this
	public bool ignore = false;

	public enum SpawnPosition
	{
		Random,
		Center
	}

	//https://www.desmos.com/calculator/waol9m1mjy
	public float GetChance(float distance)
	{
		if (startingRarity <= 0f) return 0f;

		int zone = Difficulty.DistanceBasedDifficulty(distance);
		if (zone < rarityZoneOffset) return 0f;
		int rarityZoneCutoff = rarityZoneOffset + rarityZoneCount;
		if (rarityZoneCount <= 0) return endingRarity;
		if (zone > rarityZoneCutoff) return 0f;
		
		float zoneDelta = (rarityZoneCount - rarityZoneOffset) / (float)zone + rarityZoneOffset;

		float calculation = Mathf.Lerp(startingRarity, endingRarity, zoneDelta);
		if (entityName.ToLower() == "bothive")
		{
			Debug.Log($"Distance: {distance}");
			Debug.Log($"Rarity: {startingRarity}");
			Debug.Log($"Zone: {zone}");
			Debug.Log($"Chance Calculation: {calculation}");
		}
		return calculation;
	}
}