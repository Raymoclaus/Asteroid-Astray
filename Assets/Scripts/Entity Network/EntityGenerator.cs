using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityGenerator
{
	#region Fields
	//references to all kinds of spawnable entities
	private static EntityPrefabDB prefabs;
	//store chance values from prefabs
	private static List<float> chances = new List<float>();
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private static List<List<List<bool>>> _wasFilled = new List<List<List<bool>>>();
	//List of empty game objects to store entities in and keep the hierarchy organised
	private static Dictionary<string, GameObject> holders = new Dictionary<string, GameObject>();
	//chunks to fill in batches
	private static List<ChunkCoords> chunkBatches = new List<ChunkCoords>(100);
	//maximum amount of chunks to fill per frame
	private static int maxChunkBatchFill = 1;
	private static List<SpawnableEntity> toSpawn = new List<SpawnableEntity>();
	private static bool batcherRunning = false;
	#endregion

	public static Entity FindNearestOfEntityType(EntityType type, Vector3 referenceLocation)
	{
		string entityName = type.ToString();

		GameObject holder;
		holders.TryGetValue(entityName, out holder);
		if (holder == null) return null;

		Transform holderTr = holder.transform;
		if (holderTr.childCount == 0) return SpawnEntity(type);
		float closestDist = float.PositiveInfinity;
		Transform closestTr = null;
		for (int i = 0; i < holderTr.childCount; i++)
		{
			Transform child = holderTr.GetChild(i);
			float dist = (child.position - referenceLocation).sqrMagnitude;
			if (dist < closestDist)
			{
				closestDist = dist;
				closestTr = child;
			}
			if (closestDist < 1f) break;
		}
		return closestTr.GetComponent<Entity>();
	}

	public static Entity SpawnEntity(EntityType type)
	{
		SpawnableEntity se = GetSpawnableEntity(type);
		if (se == null) return null;

		ChunkCoords cc = ClosestValidNonFilledChunk(se);
		if (cc == ChunkCoords.Invalid) return null;

		return SpawnOneInEmptyChunk(cc, se);
	}

	private static SpawnableEntity GetSpawnableEntity(EntityType type)
	{
		for (int i = 0; i < prefabs.spawnableEntities.Count; i++)
		{
			if (prefabs.spawnableEntities[i].prefab.GetEntityType() == type) return prefabs.spawnableEntities[i];
		}
		return null;
	}

	private static ChunkCoords ClosestValidNonFilledChunk(SpawnableEntity se)
	{
		int minRange = (int)((se.GetMinimumDistanceToBeSpawned() + Constants.CHUNK_SIZE / 2) / Constants.CHUNK_SIZE);
		List<ChunkCoords> coordsList = new List<ChunkCoords>();
		int count = 0;
		while (count < 100)
		{
			EntityNetwork.GetCoordsOnRangeBorder(ChunkCoords.Zero, minRange, coordsList, true);
			for (int i = 0; i < coordsList.Count; i++)
			{
				if (!Chunk(coordsList[i]) && se.GetChance(ChunkCoords.GetCenterCell(coordsList[i]).magnitude) > 0f)
				{
					return coordsList[i];
				}
			}
			coordsList.Clear();
			minRange++;
			count++;
		}
		return ChunkCoords.Invalid;
	}

	public static void FillChunk(ChunkCoords cc, bool excludePriority = false)
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
		toSpawn.Clear();
		ChooseEntitiesToSpawn(ChunkCoords.GetCenterCell(cc).magnitude, excludePriority, toSpawn);

		//determine area to spawn in
		Vector2Pair range = ChunkCoords.GetCellArea(cc);
		Vector2 spawnPos = Vector2.zero;
		for (int i = 0; i < toSpawn.Count; i++)
		{
			SpawnableEntity e = toSpawn[i];
			//determine how many to spawn
			int numToSpawn = Random.Range(e.spawnRange.A, e.spawnRange.B + 1);
			for (int j = 0; j < numToSpawn; j++)
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
				Object.Instantiate(e.prefab, spawnPos, Quaternion.identity,
					holders[e.prefab.GetEntityType().ToString()].transform);
			}
		}
	}

	public static Entity SpawnOneInEmptyChunk(ChunkCoords cc, SpawnableEntity se)
	{
		//don't bother if the given coordinates are not valid
		if (!cc.IsValid()) return null;
		//if these coordinates have no been generated yet then reserve some space for the new coordinates
		GenerateVoid(cc);
		//don't bother if the coordinates have already been filled
		if (Chunk(cc)) return null;
		//flag that this chunk coordinates was filled
		Column(cc)[cc.Y] = true;

		SpawnableEntity e = se;
		//determine how many to spawn
		Vector2Pair range = ChunkCoords.GetCellArea(cc);
		Vector2 spawnPos = Vector2.zero;
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
		return Object.Instantiate(e.prefab, spawnPos, Quaternion.identity,
			holders[e.prefab.GetEntityType().ToString()].transform);
	}

	private static List<SpawnableEntity> ChooseEntitiesToSpawn(float distance, bool excludePriority = false,
		List<SpawnableEntity> addToList = null)
	{
		addToList = addToList ?? new List<SpawnableEntity>();
		bool usingSpacePriority = false;
		//choose which non priority entities to spawn
		for (int i = 0; i < prefabs.spawnableEntities.Count; i++)
		{
			SpawnableEntity e = prefabs.spawnableEntities[i];
			if (e.ignore || (excludePriority && e.spacePriority)
				|| (usingSpacePriority && !e.spacePriority)) continue;

			float chance = Random.value;
			if (e.GetChance(distance) >= chance)
			{
				if (e.spacePriority && !usingSpacePriority)
				{
					addToList.Clear();
					usingSpacePriority = true;
				}
				addToList.Add(e);
			}
		}

		if (usingSpacePriority && addToList.Count > 0)
		{
			SpawnableEntity e = addToList[Random.Range(0, addToList.Count)];
			addToList.Clear();
			addToList.Add(e);

		}
		return addToList;
	}

	public static void InstantFillChunks(List<ChunkCoords> coords)
	{
		for (int i = 0; i < coords.Count; i++)
		{
			ChunkCoords c = coords[i];
			FillChunk(c);
		}
	}

	public static IEnumerator ChunkBatchOrder()
	{
		batcherRunning = true;
		while (true)
		{
			for (int i = 0; i < maxChunkBatchFill && chunkBatches.Count > 0; i++)
			{
				FillChunk(chunkBatches[0]);
				chunkBatches.RemoveAt(0);
			}
			if (chunkBatches.Count == 0)
			{
				batcherRunning = false;
				yield break;
			}
			yield return null;
		}
	}

	public static void EnqueueBatchOrder(List<ChunkCoords> coords, MonoBehaviour mono)
	{
		chunkBatches.AddRange(coords);
		if (!batcherRunning)
		{
			mono.StartCoroutine(ChunkBatchOrder());
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
	public static IEnumerator FillTriggerList(System.Action a)
	{
		for (int dir = 0; dir < EntityNetwork.QUADRANT_COUNT; dir++)
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
			yield return null;
		}
		a?.Invoke();
	}

	public static IEnumerator SetPrefabs(EntityPrefabDB prf, System.Action a)
	{
		prefabs = prf;
		//sort the space priority entities by lowest rarity to highest
		List<SpawnableEntity> list = prefabs.spawnableEntities;
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
		yield return null;

		for (int i = 0; i < list.Count; i++)
		{
			SpawnableEntity e = list[i];
			string name = e.prefab.GetEntityType().ToString();
			holders.Add(name, new GameObject(name));
		}
		a?.Invoke();
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