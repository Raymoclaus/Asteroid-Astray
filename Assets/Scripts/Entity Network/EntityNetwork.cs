using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomDataTypes;
using GenericExtensions;

/// Keeps track of all entities in an organised network based on their position.
/// Allows entities to be placed pseudo-infinitely in any 2D direction, although it can be upgraded to include 3D.
/// Useful in case you only want access to entities in a certain area. It's faster than checking every single one.
/// Intended for use with procedurally-generated content.
public class EntityNetwork : MonoBehaviour
{
	private static EntityNetwork instance;

	private static int QuadrantCount
		=> Enum.GetValues(typeof(Direction)).Length;
	//Determines the physical size of cells in the grid
	public const float CHUNK_SIZE = 10f;
	//network of entities
	private List<List<List<List<Entity>>>> grid = new List<List<List<List<Entity>>>>(QuadrantCount);
	//list of coordinates that contain any entities
	private HashSet<ChunkCoords> occupiedCoords = new HashSet<ChunkCoords>();
	//check if grid has already been created
	private bool gridIsSetUp = false;
	private static event Action OnLoaded;

	private List<ChunkCoords> coordsInRangeCache = new List<ChunkCoords>();

	private void Awake()
	{
		if (instance != this && instance != null)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;

		Debug.Log("Loading Entity Network");
		gridIsSetUp = true;
		Debug.Log("Entity Network Loaded");
		OnLoaded?.Invoke();
		OnLoaded = null;
	}

	public static void AddListener(Action action)
	{
		if (IsReady)
		{
			action?.Invoke();
		}
		else if (action != null)
		{
			OnLoaded += action;
		}
	}

	public static bool IsReady => instance != null && instance.gridIsSetUp;

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
		List<Entity> entitiesInCoords = addToList ?? new List<Entity>(coordsList.Count);
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

		ChunkCoords pos = new ChunkCoords(IntPair.zero, CHUNK_SIZE);
		for (pos.x = -range; pos.x <= range;)
		{
			for (pos.y = -range; pos.y <= range;)
			{
				ChunkCoords validCC = center + pos;
				if (ignoreLackOfExistenceInGrid || ChunkExists(validCC))
				{
					addToList.Add(validCC);
				}
				pos.y += pos.x <= -range || pos.x >= range ?
					1 : range * 2;
			}
			pos.x += pos.y <= -range || pos.y >= range ?
				1 : range * 2;
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

		instance.EnsureChunkExists(cc);

		Chunk(cc).Add(e);
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
			check.quadrant = (Quadrant) dir;
			//Check every column
			for (int x = 0; x < Quad(check).Count; x++)
			{
				check.x = x;
				//Check every row
				for (int y = 0; y < Column(check).Count; y++)
				{
					check.y = y;
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
		=> AccessAllEntities((Entity e) => e.DestroySelf(null, 0f));

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

		Debug.LogWarning("Entity failed to reposition on grid.");
		return false;
	}

	public static List<Entity> Chunk(ChunkCoords cc) => Column(cc)[cc.y];

	public static List<List<Entity>> Column(ChunkCoords cc) => Quad(cc)[cc.x];

	public static List<List<List<Entity>>> Quad(ChunkCoords cc)
		=> instance.grid[(int)cc.quadrant];

	/// Returns whether a given chunk is valid and exists in the network
	public static bool ChunkExists(ChunkCoords cc)
	{
		//if coordinates are invalid then chunk definitely doesn't exist
		if (!cc.IsValid() || !IsReady) return false;

		//if the quadrant doesn't have x amount of columns or that column doesn't have y amount of cells,
		//the chunk doesn't exist
		if ((int) cc.quadrant >= instance.grid.Count
		    || cc.x >= Quad(cc).Count
		    || cc.y >= Column(cc).Count) return false;

		return true;
	}

	private void EnsureChunkExists(ChunkCoords cc)
	{
		if (ChunkExists(cc)) return;

		while (grid.Count <= (int)cc.quadrant)
		{
			grid.Add(new List<List<List<Entity>>>());
		}

		while (Quad(cc).Count <= cc.x)
		{
			Quad(cc).Add(new List<List<Entity>>());
		}

		while (Column(cc).Count <= cc.y)
		{
			Column(cc).Add(new List<Entity>());
		}
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
}