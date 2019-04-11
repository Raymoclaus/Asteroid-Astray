using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
	#region Fields
	private static EntityGenerator instance;
	//references to all kinds of spawnable entities
	[SerializeField] private EntityPrefabDB prefabs;
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private List<List<List<bool>>> wasFilled = new List<List<List<bool>>>();
	//List of empty game objects to store entities in and keep the hierarchy organised
	private Dictionary<string, GameObject> holders = new Dictionary<string, GameObject>();
	//chunks to fill in batches
	private List<ChunkCoords> chunkBatches = new List<ChunkCoords>(100);
	//maximum amount of chunks to fill per frame
	private int maxChunkBatchFill = 1;
	private List<SpawnableEntity> toSpawn = new List<SpawnableEntity>();
	private bool batcherRunning = false;
	private List<bool> systemsReady = new List<bool>();

	public static bool IsReady { get { return instance != null && instance.CheckSystemsReady(); } }
	#endregion

	public delegate void CompletedSetupEventHandler();
	private static event CompletedSetupEventHandler OnCompletedSetup;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		systemsReady.Add(false);
		StartCoroutine(FillTriggerList(() =>
		{
			Ready(0);
		}));

		systemsReady.Add(false);
		StartCoroutine(SetPrefabs(() =>
		{
			Ready(1);
		}));
	}

	private void Ready(int index)
	{
		if (index >= 0 && index < systemsReady.Count)
		{
			systemsReady[index] = true;
		}

		if (CheckSystemsReady())
		{
			OnCompletedSetup?.Invoke();
			OnCompletedSetup = null;
		}
	}

	private bool CheckSystemsReady()
	{
		for (int i = 0; i < systemsReady.Count; i++)
		{
			if (!systemsReady[i]) return false;
		}
		return true;
	}

	public static void AddListener(System.Action action)
	{
		if (IsReady)
		{
			action?.Invoke();
		}
		else if (action != null)
		{
			OnCompletedSetup += new CompletedSetupEventHandler(action);
		}
	}

	public static Entity SpawnEntity(Entity e)
	{
		if (!IsReady)
		{
			AddListener(() => SpawnEntity(e));
			return null;
		}

		SpawnableEntity se = instance.GetSpawnableEntity(e);
		if (se == null) return null;

		ChunkCoords cc = instance.ClosestValidNonFilledChunk(se);
		if (cc == ChunkCoords.Invalid) return null;

		return SpawnOneInEmptyChunk(cc, se);
	}

	private SpawnableEntity GetSpawnableEntity(Entity e)
	{
		return prefabs.GetSpawnableEntity(e);
	}

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
		if (!IsReady)
		{
			AddListener(() => FillChunk(cc, excludePriority));
			return;
		}

		//don't bother if the given coordinates are not valid
		if (!cc.IsValid()) return;
		//if these coordinates have no been generated yet then reserve some space for the new coordinates
		instance.GenerateVoid(cc);
		//don't bother if the coordinates have already been filled
		if (instance.Chunk(cc)) return;
		//flag that this chunk coordinates was filled
		instance.Column(cc)[cc.Y] = true;

		//look through the space priority entities and check if one may spawn
		List<SpawnableEntity> spawnList = instance.toSpawn;
		spawnList.Clear();
		instance.ChooseEntitiesToSpawn(ChunkCoords.GetCenterCell(cc).magnitude, excludePriority, spawnList);

		//determine area to spawn in
		Vector2Pair range = ChunkCoords.GetCellArea(cc);
		Vector2 spawnPos = Vector2.zero;
		for (int i = 0; i < spawnList.Count; i++)
		{
			SpawnableEntity e = spawnList[i];
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
				Instantiate(e.prefab, spawnPos, Quaternion.identity, instance.holders[e.name].transform);
			}
		}
	}

	public static Entity SpawnOneInEmptyChunk(ChunkCoords cc, SpawnableEntity se)
	{
		if (!IsReady)
		{
			AddListener(() => SpawnOneInEmptyChunk(cc, se));
			return null;
		}

		//don't bother if the given coordinates are not valid
		if (!cc.IsValid()) return null;
		//if these coordinates have no been generated yet then reserve some space for the new coordinates
		instance.GenerateVoid(cc);
		//don't bother if the coordinates have already been filled
		if (instance.Chunk(cc)) return null;
		//flag that this chunk coordinates was filled
		instance.Column(cc)[cc.Y] = true;

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
		return Instantiate(e.prefab, spawnPos, Quaternion.identity, instance.holders[e.name].transform);
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
		if (!IsReady)
		{
			AddListener(() => InstantFillChunks(coords));
			return;
		}

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
		if (!IsReady)
		{
			AddListener(() => EnqueueBatchOrder(coords));
			return;
		}

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
			wasFilled[(int) cc.Direction][cc.X].Add(false);
		}
	}

	/// Fills up the list of fill triggers
	private IEnumerator FillTriggerList(System.Action a)
	{
		if (wasFilled.Count == 0)
		{
			for (int dir = 0; dir < EntityNetwork.QUADRANT_COUNT; dir++)
			{
				wasFilled.Add(new List<List<bool>>());
				for (int x = 0; x < EntityNetwork.RESERVE_SIZE; x++)
				{
					wasFilled[dir].Add(new List<bool>());
					for (int y = 0; y < EntityNetwork.RESERVE_SIZE; y++)
					{
						wasFilled[dir][x].Add(false);
					}
				}
				yield return null;
			}
		}
		a?.Invoke();
	}

	private IEnumerator SetPrefabs(System.Action a)
	{
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
			holders.Add(e.name, new GameObject(e.name));
		}

		a?.Invoke();
	}

	#region Convenient short-hand methods for accessing the grid
	private bool Chunk(ChunkCoords cc)
	{
		return Column(cc)[cc.Y];
	}

	private List<bool> Column(ChunkCoords cc)
	{
		return Direction(cc)[cc.X];
	}

	private List<List<bool>> Direction(ChunkCoords cc)
	{
		return wasFilled[(int) cc.Direction];
	}
	#endregion
}