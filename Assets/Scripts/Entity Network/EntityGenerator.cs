using UnityEngine.AddressableAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDataTypes;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EntityGenerator : MonoBehaviour
{
	private static EntityGenerator instance;

	//references to all kinds of spawnable entities
	private EntityPrefabDB prefabs;
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private List<List<List<bool>>> wasFilled = new List<List<List<bool>>>();
	//List of empty game objects to store entities in and keep the hierarchy organised
	private Dictionary<string, GameObject> holders = new Dictionary<string, GameObject>();
	//chunks to fill in batches
	private List<ChunkCoords> chunkBatches = new List<ChunkCoords>();
	//maximum amount of chunks to fill per frame
	private int maxChunkBatchFill = 5;
	private List<SpawnableEntity> toSpawn = new List<SpawnableEntity>();
	private bool batcherRunning = false;
	private static event System.Action OnPrefabsLoaded;

	private void Awake()
	{
		if (instance != this && instance != null)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;

		LoadPrefabs();

		SteamPunkConsole spc = FindObjectOfType<SteamPunkConsole>();
		spc?.GetCommandsFromType(GetType());
	}

	public static bool IsReady
		=> instance != null
		&& instance.prefabs != null;

	public static void AddListener(System.Action action)
	{
		if (IsReady)
		{
			action?.Invoke();
		}
		else if (action != null)
		{
			OnPrefabsLoaded += action;
		}
	}

	public static List<Entity> SpawnEntity(SpawnableEntity se, EntityData? data = null)
	{
		if (se == null) return null;

		ChunkCoords cc = instance.ClosestValidNonFilledChunk(se);
		if (cc == ChunkCoords.Invalid) return null;

		ChunkCoords emptyChunk = GetNearbyEmptyChunk();
		return SpawnEntityInChunk(se, data, emptyChunk);
	}

	public static List<Entity> SpawnEntity(Entity e)
	{
		if (e == null) return null;

		SpawnableEntity se = GetSpawnableEntity(e);
		return SpawnEntity(se);
	}

	public static List<Entity> SpawnEntity(EntityData data)
	{
		if (data.type == null) return null;

		SpawnableEntity se = GetSpawnableEntity(data.type);
		return SpawnEntity(se, data);
	}

	public static List<Entity> SpawnEntity(string entityName)
	{
		SpawnableEntity se = GetSpawnableEntity(entityName);
		return SpawnEntity(se);
	}

	public static SpawnableEntity GetSpawnableEntity(string entityName)
		=> instance.prefabs.GetSpawnableEntity(entityName);

	public static SpawnableEntity GetSpawnableEntity(Entity e) => instance.prefabs.GetSpawnableEntity(e);

	public static SpawnableEntity GetSpawnableEntity(System.Type type)
		=> instance.prefabs.GetSpawnableEntity(type);

	private ChunkCoords ClosestValidNonFilledChunk(SpawnableEntity se)
	{
		int minRange = (int)((se.GetMinimumDistanceToBeSpawned() + Constants.CHUNK_SIZE / 2) / Constants.CHUNK_SIZE);
		List<ChunkCoords> coordsList = new List<ChunkCoords>();
		int count = 0;
		while (count < 100)
		{
			EntityNetwork.GetCoordsOnRangeBorder(ChunkCoords.Zero, minRange, coordsList, true);
			for (int i = 0; i < coordsList.Count; i++)
			{
				if (!Chunk(coordsList[i]) && se.GetChance(ChunkCoords.GetCenterCell(coordsList[i], EntityNetwork.CHUNK_SIZE).magnitude) > 0f)
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
		instance.GenerateVoid(cc);
		//don't bother if the coordinates have already been filled
		if (instance.Chunk(cc)) return;
		//flag that this chunk coordinates was filled
		instance.Column(cc)[cc.y] = true;

		//look through the space priority entities and check if one may spawn
		List<SpawnableEntity> spawnList = instance.toSpawn;
		spawnList.Clear();
		instance.ChooseEntitiesToSpawn(ChunkCoords.GetCenterCell(cc, EntityNetwork.CHUNK_SIZE).magnitude, excludePriority, spawnList);

		//determine area to spawn in
		for (int i = 0; i < spawnList.Count; i++)
		{
			SpawnableEntity se = spawnList[i];
			SpawnEntityInChunk(se, null, cc);
		}
	}

	public static List<Entity> SpawnEntityInChunk(SpawnableEntity se, EntityData? data, ChunkCoords cc)
	{
		//determine how many to spawn
		int numToSpawn = Random.Range(se.spawnRange.x, se.spawnRange.y + 1);
		List<Entity> spawnedEntities = new List<Entity>(numToSpawn);
		for (int j = 0; j < numToSpawn; j++)
		{
			spawnedEntities.Add(SpawnOneEntityInChunk(se, data, cc));
		}
		return spawnedEntities;
	}

	public static Entity SpawnOneEntityInChunk(SpawnableEntity se, EntityData? data, ChunkCoords cc)
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
		//spawn it
		Entity newEntity = Instantiate(
			se.prefab,
			spawnPos,
			Quaternion.identity,
			instance.holders[se.name].transform);
		newEntity.ApplyData(data);
		return newEntity;
	}

	public static ChunkCoords GetNearbyEmptyChunk()
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
					if (!instance.Chunk(validCC)) return validCC;
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

	public static List<Entity> SpawnEntityInChunkNorthOfCamera(SpawnableEntity se, EntityData? data = null)
	{
		Vector3 cameraPos = Camera.main.transform.position;
		ChunkCoords cc = new ChunkCoords(cameraPos, EntityNetwork.CHUNK_SIZE);
		cc.y++;
		cc = cc.Validate();
		return SpawnEntityInChunk(se, data, cc);
	}

	[SteamPunkConsoleCommand(command = "Spawn", info = "Spawns named entity in chunk north of the camera.")]
	public static List<Entity> SpawnEntityInChunkNorthOfCamera(string entityName)
	{
		Vector3 cameraPos = Camera.main.transform.position;
		ChunkCoords cc = new ChunkCoords(cameraPos, EntityNetwork.CHUNK_SIZE);
		cc.y++;
		cc = cc.Validate();
		SpawnableEntity se = GetSpawnableEntity(entityName);
		return SpawnEntityInChunk(se, null, cc);
	}

	private List<SpawnableEntity> ChooseEntitiesToSpawn(float distance, bool excludePriority = false,
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

	public static void EnqueueBatchOrder(List<ChunkCoords> coords)
	{
		instance.chunkBatches.AddRange(coords);
		if (!instance.batcherRunning)
		{
			instance.StartCoroutine(instance.ChunkBatchOrder());
		}
	}

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

	private void LoadPrefabs()
	{
		Debug.Log("Loading Entity Generator");
		AsyncOperationHandle<EntityPrefabDB> handle
			= Addressables.LoadAssetAsync<EntityPrefabDB>("EntityPrefabDB");
		handle.Completed += SetPrefabs;
	}

	private void SetPrefabs(AsyncOperationHandle<EntityPrefabDB> handle)
	{
		prefabs = handle.Result;
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

		for (int i = 0; i < list.Count; i++)
		{
			SpawnableEntity e = list[i];
			holders.Add(e.name, new GameObject(e.name));
		}

		Debug.Log("Entity Generator Loaded");
		OnPrefabsLoaded?.Invoke();
		OnPrefabsLoaded = null;
	}

	private bool Chunk(ChunkCoords cc) => Column(cc)[cc.y];

	private List<bool> Column(ChunkCoords cc) => Quad(cc)[cc.x];

	private List<List<bool>> Quad(ChunkCoords cc) => wasFilled[(int)cc.quadrant];
}