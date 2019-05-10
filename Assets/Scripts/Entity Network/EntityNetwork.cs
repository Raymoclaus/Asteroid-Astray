﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

/// Keeps track of all entities in an organised network based on their position.
/// Allows entities to be placed pseudo-infinitely in any 2D direction, although it can be upgraded to include 3D.
/// Useful in case you only want access to entities in a certain area. It's faster than checking every single one.
/// Intended for use with procedurally-generated content.
public class EntityNetwork : MonoBehaviour
{
	private static EntityNetwork instance;

	[SerializeField] private string saveKey;
	private List<EntityData> savedEntities = new List<EntityData>();

	#region Fields
	//4 directions in the grid
	public const int QUADRANT_COUNT = 4;
	//Determines the physical size of cells in the grid
	public const float CHUNK_SIZE = 10f;
	//network of entities
	private List<List<List<List<Entity>>>> grid = new List<List<List<List<Entity>>>>(QUADRANT_COUNT);
	//list of coordinates that contain any entities
	private List<ChunkCoords> occupiedCoords = new List<ChunkCoords>(RESERVE_SIZE * RESERVE_SIZE * 4);
	//some large number of reserve to avoid List resizing lag
	public const int RESERVE_SIZE = 1000;
	//another large number for each individual cell in the grid
	public const int CELL_RESERVE_SIZE = 15;
	//check if grid has already been created
	private bool gridIsSetUp = false;
	public static bool IsReady { get { return instance != null && instance.gridIsSetUp; } }
	#endregion

	#region Stat Tracking
	//Used to keep track of the amount of active entities. Debugging tool
	private int numEntities;
	#endregion

	#region Caching
	private List<ChunkCoords> coordsInRangeCache = new List<ChunkCoords>();
	#endregion

	public delegate void GridSetUpEventHandler();
	private static event GridSetUpEventHandler OnGridSetUp;

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

		GameEvents.OnSave += Save;
		OnGridSetUp += Load;
		StartCoroutine(CreateGrid());
	}

	public static void AddListener(Action action)
	{
		if (IsReady)
		{
			action?.Invoke();
		}
		else if (action != null)
		{
			OnGridSetUp += new GridSetUpEventHandler(action);
		}
	}

	/// Constructs and reserves a large amount of space for the grid
	private IEnumerator CreateGrid()
	{
		if (!gridIsSetUp)
		{
			//reserve space in each direction
			//takes ~1.5 seconds for 1000 * 1000 * 10
			for (int dir = 0; dir < QUADRANT_COUNT; dir++)
			{
				grid.Add(new List<List<List<Entity>>>(RESERVE_SIZE));
				for (int x = 0; x < RESERVE_SIZE; x++)
				{
					grid[dir].Add(new List<List<Entity>>(RESERVE_SIZE));
					for (int y = 0; y < RESERVE_SIZE; y++)
					{
						grid[dir][x].Add(new List<Entity>(CELL_RESERVE_SIZE));
					}
				}
				yield return null;
			}

			gridIsSetUp = true;
			OnGridSetUp?.Invoke();
			OnGridSetUp = null;
		}
	}

	/// Returns a list of all entities located in cells within range of the given coordinates
	public static List<Entity> GetEntitiesInRange(ChunkCoords center, int range, EntityType? type = null,
		List<Entity> exclusions = null, List<Entity> addToList = null)
	{
		instance.coordsInRangeCache.Clear();
		List<ChunkCoords> coordsInRange = GetCoordsInRange(center, range, instance.coordsInRangeCache);
		//declare a list to be filled and reserve some room
		return GetEntitiesAtCoords(coordsInRange, type, exclusions, addToList);
	}

	/// Returns a list of entities located in specified coordinates
	public static List<Entity> GetEntitiesAtCoords(List<ChunkCoords> coordsList, EntityType? type = null,
		List<Entity> exclusions = null, List<Entity> addToList = null)
	{
		List<Entity> entitiesInCoords = addToList ?? new List<Entity>(CELL_RESERVE_SIZE * coordsList.Count);
		//loop through coordinates list and grab all entities at each coordinate
		for (int i = 0; i < coordsList.Count; i++)
		{
			ChunkCoords coord = coordsList[i];
			if (type == null)
			{
				entitiesInCoords.AddRange(Chunk(coord));
			}
			else
			{
				EntityType filter = (EntityType)type;
				for (int j = 0; j < Chunk(coord).Count; j++)
				{
					Entity e = Chunk(coord)[j];
					if (e.GetEntityType() == filter && !EntityIsInSet(e, exclusions))
					{
						entitiesInCoords.Add(e);
					}
				}
			}
		}
		return entitiesInCoords;
	}

	/// Iterates over a given list and checks if a given entity is within the set
	private static bool EntityIsInSet(Entity e, List<Entity> set)
	{
		if (set == null) return false;

		for (int i = 0; i < set.Count; i++)
		{
			Entity entity = set[i];
			if (entity == e) return true;
		}
		return false;
	}

	/// Returns a list of coordinates around a given center coordinate in a specified range
	public static List<ChunkCoords> GetCoordsInRange(ChunkCoords center, int range,
		List<ChunkCoords> coordsInRange = null, bool ignoreLackOfExistenceInGrid = false)
	{
		int r = range + 2;
		coordsInRange = coordsInRange ?? new List<ChunkCoords>(r * r);
		//loop through surrounding chunks
		for (int i = 0; i <= range; i++)
		{
			GetCoordsOnRangeBorder(center, i, coordsInRange, ignoreLackOfExistenceInGrid);
		}

		return coordsInRange;
	}

	/// Returns a list of coordinates that are a specified distance from a given center coordinate
	public static List<ChunkCoords> GetCoordsOnRangeBorder(ChunkCoords center, int range,
		List<ChunkCoords> addToList = null, bool ignoreLackOfExistenceInGrid = false)
	{
		int r = range * 8;
		addToList = addToList ?? new List<ChunkCoords>(r);
		ChunkCoords c = center;
		//loop through surrounding chunks
		for (int i = -range; i <= range; i++)
		{
			c.X = center.X + i;
			for (int j = -range; j <= range; j++)
			{
				if (Math.Abs(i) != range && Math.Abs(j) != range) continue;

				c.Y = center.Y + j;
				//validate will adjust for edge cases
				ChunkCoords validCc = c;
				validCc.Validate();
				if (ignoreLackOfExistenceInGrid || ChunkExists(validCc))
				{
					addToList.Add(validCc);
				}
			}
		}

		return addToList;
	}

	/// Adds a given entity to the list at given coordinates
	public static bool AddEntity(Entity e, ChunkCoords cc)
	{
		if (!cc.IsValid())
		{
			Debug.LogWarning("Coordinates to add entity to are invalid.");
			return false;
		}

		Chunk(cc).Add(e);
		instance.numEntities++;
		//update list of occupied coordinates
		if (Chunk(cc).Count == 1)
		{
			instance.occupiedCoords.Add(cc);
		}
		//set entity's coordinates to be equal to the given coordinates
		e.SetCoordinates(cc);
		return true;
	}

	/// Removes an entity from the network
	public static bool RemoveEntity(Entity e, EntityType? type = null)
	{
		ChunkCoords cc = e.GetCoords();
		if (!ChunkExists(cc)) return false;

		List<Entity> chunk = Chunk(cc);
		for (int i = 0; i < chunk.Count; i++)
		{
			if (chunk[i] == e)
			{
				chunk.RemoveAt(i);
				instance.numEntities--;
				if (chunk.Count == 0)
				{
					instance.occupiedCoords.Remove(cc);
				}
				return true;
			}
		}

		return false;
	}

	/// Iterates through all entities and performs the action once for each entity
	public static void AccessAllEntities(Action<Entity> act, EntityType? onlyType = null)
	{
		ChunkCoords check = ChunkCoords.Zero;
		bool limitCheck = onlyType != null;
		//Check every direction
		for (int dir = 0; dir < instance.grid.Count; dir++)
		{
			check.Direction = (Quadrant) dir;
			//Check every column
			for (int x = 0; x < Direction(check).Count; x++)
			{
				check.X = x;
				//Check every row
				for (int y = 0; y < Column(check).Count; y++)
				{
					check.Y = y;
					//Check every entity in chunk
					for (int i = Chunk(check).Count; i >= 0; i--)
					{
						Entity e = Chunk(check)[i];
						//only access entities with the given type (any if no type is given)
						if ((limitCheck && e.GetEntityType() == (EntityType) onlyType) || !limitCheck)
						{
							act(e);
						}
					}
				}
			}
		}
	}

	public static void DestroyAllEntities()
	{
		AccessAllEntities((Entity e) => e.DestroySelf(null));
	}

	/// Removes an entity from its position in the network and replaces it and the given destination
	/// This will mostly be used by entities themselves as they are responsible for determining their place in the network
	public static bool Reposition(Entity e, ChunkCoords destChunk)
	{
		if (!destChunk.IsValid())
		{
			Debug.LogWarning("Destination coordinates are invalid.");
			return false;
		}

		if (RemoveEntity(e)) return AddEntity(e, destChunk);
		
		//search for entity if removal coordinates are incorrect or if entity was not found at its coordinates
		ChunkCoords found = StartFullSearch(e);
		//search will return ChunkCoordinates.Invalid if it was not found anywhere in the network
		if (found == ChunkCoords.Invalid)
		{
			Debug.LogWarning("Entity failed to reposition on grid.");
			return false;
		}

		Chunk(found).Remove(e);

		//add entity to the destination chunk
		return AddEntity(e, destChunk);
	}
	
	#region Convenient Grid Methods

	public static List<Entity> Chunk(ChunkCoords cc)
	{
		return Column(cc)[cc.Y];
	}

	public static List<List<Entity>> Column(ChunkCoords cc)
	{
		return Direction(cc)[cc.X];
	}

	public static List<List<List<Entity>>> Direction(ChunkCoords cc)
	{
		return instance.grid[(int) cc.Direction];
	}

	#endregion

	/// Begins a search and outputs some debug information
	private static ChunkCoords StartFullSearch(Entity e)
	{
		Debug.LogWarning("Starting full search for: " + e);
		ChunkCoords search = ChunkCoords.Zero;
		FunctionTimer timer = new FunctionTimer();
		bool found = false;
		for (int i = 0; i < instance.grid.Count; i++)
		{
			List<List<List<Entity>>> dir = instance.grid[i];
			for (int j = 0; j < dir.Count; j++)
			{
				List<List<Entity>> col = dir[j];
				for (int k = 0; k < col.Count; k++)
				{
					List<Entity> chunk = col[k];
					for (int l = 0; l < chunk.Count; l++)
					{
						Entity ent = chunk[l];
						if (ent == e)
						{
							found = true;
						}
					}
					if (found) break;
					else search.Y++;
				}
				if (found) break;
				else search.X++;
			}
			if (found) break;
			else search.Direction++;
		}
		Debug.LogWarning($"Full search for: {e} completed {(found ? "" : "un")}successfully in {timer.Log()} seconds.");
		return search;
	}

	/// Returns whether a given chunk is valid and exists in the network
	public static bool ChunkExists(ChunkCoords cc)
	{
		//if coordinates are invalid then chunk definitely doesn't exist
		if (!cc.IsValid() || !IsReady) return false;

		//if the quadrant doesn't have x amount of columns or that column doesn't have y amount of cells,
		//the chunk doesn't exist
		if ((int) cc.Direction >= instance.grid.Count
		    || cc.X >= Direction(cc).Count
		    || cc.Y >= Column(cc).Count) return false;

		return true;
	}

	/// Returns whether an entity is at a specific location
	public static bool ConfirmLocation(Entity e, ChunkCoords c)
	{
		return EntityIsInSet(e, Chunk(c));
	}

	/// Returns whether a chunk contains any entities of a certain type
	public static bool ContainsType(EntityType type, ChunkCoords c, Entity entToExclude = null)
	{
		if (!ChunkExists(c)) return false;

		for (int i = 0; i < Chunk(c).Count; i++)
		{
			Entity e = Chunk(c)[i];
			if (e.GetEntityType() == type && e != entToExclude) return true;
		}
		return false;
	}

	public static int GetEntityCount()
	{
		return instance.numEntities;
	}

	public static void AddToSavedEntities(EntityData data)
	{
		instance.savedEntities.Add(data);
	}

	public void Save()
	{
		if (saveKey == null || saveKey == string.Empty) return;
		Coroutines.TimedAction(0.1f, null, () =>
		{
			SaveLoad.Save(savedEntities, saveKey);
		});
	}

	public void Load()
	{
		List<EntityData> loadedEntities = SaveLoad.Load<List<EntityData>>(saveKey);
		for (int i = 0; loadedEntities != null && i < loadedEntities.Count; i++)
		{
			EntityGenerator.SpawnEntity(loadedEntities[i]);
		}
		OnGridSetUp -= Load;
	}
}