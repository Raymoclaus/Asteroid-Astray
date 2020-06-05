using CustomDataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityGenerator : MonoBehaviour
{
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private List<List<List<bool>>> wasFilled = new List<List<List<bool>>>();
	//List of empty game objects to store entities in and keep the hierarchy organised
	private Dictionary<string, GameObject> holders = new Dictionary<string, GameObject>();
	//chunks to fill in batches
	private List<ChunkCoords> chunkBatches = new List<ChunkCoords>();
	//maximum amount of chunks to fill per frame
	private int maxChunkBatchFill = 5;
	private bool batcherRunning = false;
	private EntityNetwork _entityNetwork;
	public InvocableOneShotEvent OnPrefabsLoaded = new InvocableOneShotEvent();

	private void Awake()
	{
		LoadPrefabs();
		
		SteamPunkConsole.GetCommandsFromType(GetType());
	}

	private EntityNetwork EntityNetwork
	{
		get
		{
			if (_entityNetwork != null && !_entityNetwork.Equals(null)) return _entityNetwork;
			return _entityNetwork = FindObjectOfType<EntityNetwork>();
		}
	}

	public static bool IsReady
		=> EntityPrefabLoader.IsReady;

	public List<Entity> SpawnEntity(SpawnableEntity se)
	{
		if (se == null) return null;

		ChunkCoords cc = ClosestValidNonFilledChunk(se);
		if (cc == ChunkCoords.Invalid) return null;

		ChunkCoords emptyChunk = GetNearbyEmptyChunk();
		return SpawnEntityInChunk(se, emptyChunk);
	}

	public List<Entity> SpawnEntity(Entity e)
	{
		if (e == null) return null;

		SpawnableEntity se = GetSpawnableEntityByPrefabReference(e);
		return SpawnEntity(se);
	}

	public List<Entity> SpawnEntity(string entityName)
	{
		SpawnableEntity se = GetSpawnableEntityByEntityName(entityName);
		return SpawnEntity(se);
	}

	public static SpawnableEntity GetSpawnableEntityByEntityName(string entityName)
		=> EntityPrefabLoader.GetSpawnableEntityByEntityName(entityName);

	public static SpawnableEntity GetSpawnableEntityByPrefabReference(Entity e)
		=> EntityPrefabLoader.GetSpawnableEntityByPrefabReference(e);

	public static SpawnableEntity GetSpawnableEntityByPrefabType(Type type)
		=> EntityPrefabLoader.GetSpawnableEntityByPrefabType(type);

	public static SpawnableEntity GetSpawnableEntityByFileName(string filename)
		=> EntityPrefabLoader.GetSpawnableEntityByFileName(filename);

	private ChunkCoords ClosestValidNonFilledChunk(SpawnableEntity se)
	{
		int minRange = Mathf.Max(0, se.rarityZoneOffset);
		ChunkCoords coords = ChunkCoords.Invalid;
		while (coords == ChunkCoords.Invalid)
		{
			EntityNetwork.IterateCoordsOnRangeBorder(ChunkCoords.Zero, minRange,
				cc =>
				{
					if (!Chunk(cc))
					{
						coords = cc;
						return true;
					}

					return false;
				});
			minRange++;
		}
		return coords;
	}

	public void FillChunk(ChunkCoords cc, bool excludePriority = false)
	{
		//don't bother if the given coordinates are not valid
		if (!cc.IsValid()) return;
		//don't bother if the coordinates have already been filled
		if (Chunk(cc)) return;
		//flag that this chunk coordinates was filled
		Column(cc)[cc.y] = true;

		//look through the space priority entities and check if one may spawn
		SpawnableEntity se = ChooseEntityToSpawn(
			ChunkCoords.GetCenterCell(cc, EntityNetwork.CHUNK_SIZE).magnitude);
		SpawnEntityInChunkNonAlloc(se, cc);
	}

	public List<Entity> SpawnEntityInChunk(SpawnableEntity se, ChunkCoords cc)
	{
		if (se == null) return null;
		//determine how many to spawn
		int numToSpawn = Random.Range(se.minSpawnCountInChunk, se.maxSpawnCountInChunk + 1);
		List<Entity> spawnedEntities = new List<Entity>(numToSpawn);
		for (int j = 0; j < numToSpawn; j++)
		{
			spawnedEntities.Add(SpawnOneEntityInChunk(se, cc));
		}
		return spawnedEntities;
	}

	public void SpawnEntityInChunkNonAlloc(SpawnableEntity se, ChunkCoords cc)
	{
		if (se == null) return;
		//determine how many to spawn
		int numToSpawn = Random.Range(se.minSpawnCountInChunk, se.maxSpawnCountInChunk + 1);
		for (int j = 0; j < numToSpawn; j++)
		{
			SpawnOneEntityInChunk(se, cc);
		}
	}

	public Entity SpawnOneEntityInChunk(SpawnableEntity se, ChunkCoords cc)
	{
		//pick a position within the chunk coordinates
		Vector2 spawnPos = Vector2.zero;
		Vector2Pair range = ChunkCoords.GetCellArea(cc, EntityNetwork.CHUNK_SIZE);
		switch (se.posType)
		{
			case SpawnableEntity.SpawnPosition.Random:
				spawnPos.x = Random.Range(range.a.x, range.b.x);
				spawnPos.y = Random.Range(range.a.y, range.b.y);
				break;
			case SpawnableEntity.SpawnPosition.Center:
				spawnPos = ChunkCoords.GetCenterCell(cc, EntityNetwork.CHUNK_SIZE);
				break;
		}
		return InstantiateEntity(se, spawnPos);
	}

	public Entity InstantiateEntity(SpawnableEntity se, Vector2 spawnPos)
	{
		//spawn it
		Entity newEntity = Instantiate(
			se.prefab,
			spawnPos,
			Quaternion.identity,
			holders[se.entityName].transform);
		newEntity.PrefabID = se.name;
		return newEntity;
	}

	public ChunkCoords GetNearbyEmptyChunk()
	{
		int range = 0;
		ChunkCoords pos = new ChunkCoords(IntPair.zero, EntityNetwork.CHUNK_SIZE);
		while (range < int.MaxValue)
		{
			for (pos.x = -range; pos.x <= range;)
			{
				for (pos.y = -range; pos.y <= range;)
				{
					ChunkCoords validCC = pos.Validate();
					if (!ChunkExists(validCC) || !Chunk(validCC)) return validCC;
					pos.y += pos.x <= -range || pos.x >= range ?
						1 : range * 2;
				}
				pos.x += pos.y <= -range || pos.y >= range ?
					1 : range * 2;
			}
			range++;
		}
		return ChunkCoords.Invalid;
	}

	public List<Entity> SpawnEntityInChunkNorthOfCamera(SpawnableEntity se)
	{
		Vector3 cameraPos = Camera.main.transform.position;
		ChunkCoords cc = new ChunkCoords(cameraPos, EntityNetwork.CHUNK_SIZE);
		cc.y++;
		cc = cc.Validate();
		return SpawnEntityInChunk(se, cc);
	}

	[SteamPunkConsoleCommand(command = "Spawn", info = "Spawns named entity in chunk north of the camera.")]
	public List<Entity> SpawnEntityInChunkNorthOfCamera(string entityName)
	{
		Vector3 cameraPos = Camera.main.transform.position;
		ChunkCoords cc = new ChunkCoords(cameraPos, EntityNetwork.CHUNK_SIZE);
		cc.y++;
		cc = cc.Validate();
		SpawnableEntity se = GetSpawnableEntityByEntityName(entityName);
		return SpawnEntityInChunk(se, cc);
	}

	private SpawnableEntity ChooseEntityToSpawn(float distance)
	{
		List<SpawnableEntityChance> validPrefabs = GetPrefabs(distance);
		float totalRarity = SpawnableEntityChance.GetTotalChance(validPrefabs);
		float randomChoose = Random.Range(0f, totalRarity);

		//choose which non priority entities to spawn
		for (int i = 0; i < validPrefabs.Count; i++)
		{
			SpawnableEntityChance sec = validPrefabs[i];
			SpawnableEntity se = sec.entity;
			float chance = sec.chance;
			randomChoose -= chance;
			if (randomChoose <= 0f) return se;
		}
		Debug.Log("No Spawnable Entity Chosen");
		return null;
	}

	private List<SpawnableEntity> Prefabs => EntityPrefabLoader.spawnableEntities;

	private List<SpawnableEntityChance> cachedChances = new List<SpawnableEntityChance>();
	private List<SpawnableEntityChance> GetPrefabs(float distance)
	{
		cachedChances.Clear();
		for (int i = 0; i < Prefabs.Count; i++)
		{
			SpawnableEntity se = Prefabs[i];
			if (se.ignore) continue;
			float chance = se.GetChance(distance);
			if (chance == 0f) continue;
			cachedChances.Add(new SpawnableEntityChance(se, chance));
		}
		return cachedChances;
	}

	public void InstantFillChunks(List<ChunkCoords> coords)
	{
		for (int i = 0; i < coords.Count; i++)
		{
			ChunkCoords c = coords[i];
			FillChunk(c);
		}
	}

	private IEnumerator ChunkBatchOrder()
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

	public void EnqueueBatchOrder(List<ChunkCoords> coords)
	{
		chunkBatches.AddRange(coords);
		if (!batcherRunning)
		{
			StartCoroutine(ChunkBatchOrder());
		}
	}

	private void LoadPrefabs()
	{
		Debug.Log("Loading Entity Generator");
		EntityPrefabLoader.OnPrefabsLoaded += CreateHolders;
		EntityPrefabLoader.LoadPrefabs();
	}

	private void CreateHolders()
	{
		for (int i = 0; i < Prefabs.Count; i++)
		{
			SpawnableEntity e = Prefabs[i];
			if (holders.ContainsKey(e.entityName)) continue;
			holders.Add(e.entityName, new GameObject(e.entityName));
		}

		Debug.Log("Entity Generator Loaded");
		OnPrefabsLoaded?.Invoke();
		OnPrefabsLoaded = null;
	}

	private bool ChunkExists(ChunkCoords cc)
	{
		if (cc.quadrant < 0 || (int)cc.quadrant >= wasFilled.Count) return false;
		if (cc.x < 0 || cc.x >= Quad(cc).Count) return false;
		if (cc.y < 0 || cc.y >= Column(cc).Count) return false;
		return true;
	}

	private bool Chunk(ChunkCoords cc)
	{
		try
		{
			return Column(cc)[cc.y];
		}
		catch (ArgumentOutOfRangeException e)
		{
			GenerateVoid(cc);
			return Chunk(cc);
		}
	}

	private List<bool> Column(ChunkCoords cc) => Quad(cc)[cc.x];

	private List<List<bool>> Quad(ChunkCoords cc) => wasFilled[(int)cc.quadrant];

	/// Increases capacity of the fill trigger list to accomodate given coordinates
	private void GenerateVoid(ChunkCoords cc)
	{
		//ignore if given coordinates are invalid or they already exist
		if (!cc.IsValid()) return;

		//add more quadrants until enough exist to make the given coordinates valid
		while (wasFilled.Count <= (int)cc.quadrant)
		{
			wasFilled.Add(new List<List<bool>>());
		}

		//add more quadrants until enough exist to make the given coordinates valid
		while (Quad(cc).Count <= cc.x)
		{
			Quad(cc).Add(new List<bool>());
		}

		//add more quadrants until enough exist to make the given coordinates valid
		while (Column(cc).Count <= cc.y)
		{
			Column(cc).Add(false);
		}
	}

	private struct SpawnableEntityChance
	{
		public SpawnableEntity entity;
		public float chance;

		public SpawnableEntityChance(SpawnableEntity se, float chance)
		{
			this.entity = se;
			this.chance = chance;
		}

		public static float GetTotalChance(List<SpawnableEntityChance> seC)
		{
			float chance = 0f;
			for (int i = 0; i < seC.Count; i++)
			{
				chance += seC[i].chance;
			}
			return chance;
		}
	}
}