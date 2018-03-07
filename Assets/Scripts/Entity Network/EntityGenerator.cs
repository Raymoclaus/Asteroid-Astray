﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityGenerator
{
	#region Fields
	//references to all kinds of spawnable entities
	private static EntityPrefabController prefabs;
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private static List<List<List<bool>>> _wasFilled = new List<List<List<bool>>>();
	//List of empty game objects to store entities in and keep the hierarchy organised
	private static Dictionary<string, GameObject> holders = new Dictionary<string, GameObject>();
	//chunks to fill in batches
	private static List<ChunkCoords> chunkBatches = new List<ChunkCoords>(100);
	//minimum amount of chunks to fill per batch
	private static int minChunkBatchFill = 5;
	#endregion

	public static void FillChunk(ChunkCoords cc, bool? excludePriority = null)
	{
		//don't bother if the given coordinates are not valid
		if (!cc.IsValid()) return;
		//if these coordinates have no been generated yet then reserve some space for the new coordinates
		GenerateVoid(cc);
		//don't bother if the coordinates have already been filled
		if (Chunk(cc)) return;
		//flag that this chunk coordinates was filled
		Column(cc)[cc.Y] = true;

		//look through the space priority entities and check if one may spawn
		List<SpawnableEntity> toSpawn = ChooseEntitiesToSpawn(excludePriority);

		//determine area to spawn in
		Vector2Pair range = ChunkCoords.GetCellArea(cc);
		Vector2 spawnPos = Vector2.zero;
		foreach (SpawnableEntity e in toSpawn)
		{
			//determine how many to spawn
			int numToSpawn = Random.Range(e.spawnRange.A, e.spawnRange.B + 1);
			for (int i = 0; i < numToSpawn; i++)
			{
				//pick a position within the chunk coordinates
				switch (e.posType)
				{
					case SpawnableEntity.SpawnPosition.Random:
						spawnPos.x = Random.Range(range.A.x, range.B.x);
						spawnPos.y = Random.Range(range.A.y, range.B.y);
						break;
					case SpawnableEntity.SpawnPosition.Center:
						spawnPos = ChunkCoords.GetCenterCell(cc);
						break;
				}
				//spawn it
				Object.Instantiate(e.prefab, spawnPos, Quaternion.identity, holders[e.name].transform);
			}
		}
	}

	private static List<SpawnableEntity> ChooseEntitiesToSpawn(bool? excludePriority = null)
	{
		bool noPriority = excludePriority ?? false;
		List<SpawnableEntity> list = new List<SpawnableEntity>();
		float chance = Random.value;
		if (!noPriority)
		{
			foreach (SpawnableEntity e in prefabs.spacePriorityEntities)
			{
				if (e.ignore) continue;

				if (e.rarity >= chance)
				{
					list.Add(e);
					break;
				}
			}
		}

		//if a priority entity was chosen then only return that
		if (list.Count > 0) return list;

		//choose which non priority entities to spawn
		foreach (SpawnableEntity e in prefabs.spawnableEntities)
		{
			if (e.ignore) continue;

			if (e.rarity >= chance)
			{
				list.Add(e);
			}
		}

		return list;
	}

	public static void InstantFillChunks(List<ChunkCoords> coords)
	{
		foreach(ChunkCoords c in coords)
		{
			FillChunk(c);
		}
	}

	public static IEnumerator ChunkBatchOrder(List<ChunkCoords> coords)
	{
		bool executing = chunkBatches.Count > 0;
		chunkBatches.AddRange(coords);
		if (executing)
		{
			yield break;
		}
		while (true)
		{
			if (chunkBatches.Count == 0) yield return null;
			for (int i = 0; i < minChunkBatchFill && chunkBatches.Count > 0; i++)
			{
				FillChunk(chunkBatches[0]);
				chunkBatches.RemoveAt(0);
			}
			yield return null;
		}
	}

	/// Increases capacity of the fill trigger list to accomodate given coordinates
	private static void GenerateVoid(ChunkCoords cc)
	{
		//ignore if given coordinates are invalid or they already exist
		if (!cc.IsValid() || EntityNetwork.ChunkExists(cc))
		{
			return;
		}

		//add more columns until enough exist to make the given coordinates valid
		if (Direction(cc).Capacity <= cc.X)
		{
			Debug.LogWarning("Row capacity breached.");
			Direction(cc).Capacity = cc.X + 1;
		}

		while (Direction(cc).Count <= cc.X)
		{
			Direction(cc).Add(new List<bool>());
		}

		//add more rows until the column is large enough to make the given coordinates valid
		if (Column(cc).Capacity <= cc.Y)
		{
			Debug.LogWarning("Column capacity breached.");
			Column(cc).Capacity = cc.Y + 1;
		}

		while (Column(cc).Count <= cc.Y)
		{
			_wasFilled[(int) cc.Direction][cc.X].Add(false);
		}
	}

	/// Fills up the list of fill triggers
	public static void FillTriggerList()
	{
		for (int dir = 0; dir < EntityNetwork.QuadrantNumber; dir++)
		{
			_wasFilled.Add(new List<List<bool>>());
			for (int x = 0; x < EntityNetwork.ReserveSize; x++)
			{
				_wasFilled[dir].Add(new List<bool>());
				for (int y = 0; y < EntityNetwork.ReserveSize; y++)
				{
					_wasFilled[dir][x].Add(false);
				}
			}
		}
	}

	/// Removes and destroys all asteroids in the entity network then sets all fill triggers to false
	public static void DestroyAllAsteroids()
	{
		//destroy all asteroid entities
		EntityNetwork.DestroyAllEntities(EntityType.Asteroid);

		//set all fill triggers to false
		ChunkCoords check = ChunkCoords.Zero;
		for (int dir = 0; dir < _wasFilled.Count; dir++)
		{
			check.Direction = (Quadrant) dir;
			for (int x = 0; x < Direction(check).Count; x++)
			{
				check.X = x;
				for (int y = 0; y < Column(check).Count; y++)
				{
					Column(check)[y] = false;
				}
			}
		}
	}

	public static void SetPrefabs(EntityPrefabController prf)
	{
		prefabs = prf;
		//sort the space priority entities by lowest rarity to highest
		List<SpawnableEntity> list = prefabs.spacePriorityEntities;
		for (int i = 1; i < list.Count; i += 0)
		{
			SpawnableEntity e = list[i];
			if (e.rarity < list[i - 1].rarity)
			{
				list.RemoveAt(i);
				list.Insert(i - 1, e);
				i -= i > 1 ? 1 : 0;
			}
			else
			{
				i++;
			}
		}

		foreach (SpawnableEntity e in list)
		{
			holders.Add(e.name, new GameObject(e.name));
		}

		foreach (SpawnableEntity e in prefabs.spawnableEntities)
		{
			holders.Add(e.name, new GameObject(e.name));
		}
	}

	#region Convenient short-hand methods for accessing the grid
	private static bool Chunk(ChunkCoords cc)
	{
		return Column(cc)[cc.Y];
	}

	private static List<bool> Column(ChunkCoords cc)
	{
		return Direction(cc)[cc.X];
	}

	private static List<List<bool>> Direction(ChunkCoords cc)
	{
		return _wasFilled[(int) cc.Direction];
	}
	#endregion
}