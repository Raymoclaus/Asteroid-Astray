using System;
using System.Collections.Generic;
using UnityEngine;
using CustomDataTypes;
using SaveSystem;
using UnityEditor;

/// Keeps track of all entities in an organised network based on their position.
/// Allows entities to be placed pseudo-infinitely in any 2D direction, although it can be upgraded to include 3D.
/// Useful in case you only want access to entities in a certain area. It's faster than checking every single one.
/// Intended for use with procedurally-generated content.
public class EntityNetwork : MonoBehaviour
{
	private static EntityNetwork instance;
	private static int QuadrantCount
		=> ChunkCoords.DIRECTION_COUNT;
	//Determines the physical size of cells in the grid
	public const float CHUNK_SIZE = 10f;
	//network of entities
	private List<List<List<List<Entity>>>> grid = new List<List<List<List<Entity>>>>(QuadrantCount);
	//list of coordinates that contain any entities
	private HashSet<ChunkCoords> occupiedCoords = new HashSet<ChunkCoords>();
	//check if grid has already been created
	private bool gridIsSetUp = false;
	//entities to query when saving
	private HashSet<Entity> toSave = new HashSet<Entity>();
	//list of entities sorted by type
	private Dictionary<Type, HashSet<Entity>> entitiesByType = new Dictionary<Type, HashSet<Entity>>();

	private static event Action OnLoaded;

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
	public static bool IterateEntitiesInRange(ChunkCoords center, int range, Func<Entity, bool> action)
	{
		return IterateCoordsInRange(
			center,
			range,
			cc =>
			{
				return IterateEntitiesAtCoord(cc, action);
			},
			false);
	}

	public static bool IterateEntitiesAtCoord(ChunkCoords coord, Func<Entity, bool> action)
	{
		if (!ChunkExists(coord)) return false;
		List<Entity> entities = Chunk(coord);
		for (int i = 0; i < entities.Count; i++)
		{
			if (action(entities[i])) return true;
		}

		return false;
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
	public static bool IterateCoordsInRange(ChunkCoords center, int range,
		Func<ChunkCoords, bool> action, bool ignoreLackOfExistenceInGrid)
	{
		int r = range + 2;
		//loop through surrounding chunks
		for (int i = 0; i <= range; i++)
		{
			if (IterateCoordsOnRangeBorder(center, i, action, ignoreLackOfExistenceInGrid))
			{
				return true;
			}
		}

		return false;
	}

	/// Returns a list of coordinates that are a specified distance from a given center coordinate
	public static bool IterateCoordsOnRangeBorder(ChunkCoords center, int range,
		Func<ChunkCoords, bool> exitCondition, bool ignoreLackOfExistenceInGrid)
	{
		int r = range * 8;

		ChunkCoords pos = new ChunkCoords(IntPair.zero, CHUNK_SIZE);
		for (pos.x = -range; pos.x <= range;)
		{
			for (pos.y = -range; pos.y <= range;)
			{
				ChunkCoords validCC = (center + pos).Validate();
				if (ignoreLackOfExistenceInGrid || ChunkExists(validCC))
				{
					if (exitCondition?.Invoke(validCC) ?? false) return true;
				}
				pos.y += pos.x <= -range || pos.x >= range ?
					1 : range * 2;
			}
			pos.x += pos.y <= -range || pos.y >= range ?
				1 : range * 2;
		}

		return false;
	}

	/// Adds a given entity to the list at given coordinates
	public static bool AddEntity(Entity e, ChunkCoords cc)
	{
		//check if the given coordinates are valid
		if (!cc.IsValid())
		{
			Debug.LogWarning("Coordinates to add entity to are invalid.");
			return false;
		}
		//make sure that the chunk with those coordinates exists
		instance.EnsureChunkExists(cc);
		//add entity to that chunk
		Chunk(cc).Add(e);
		//add entity to type-sorted list
		instance.AddEntityToTypeSortedList(e);
		//update list of occupied coordinates
		if (Chunk(cc).Count == 1)
		{
			instance.occupiedCoords.Add(cc);
		}
		//set entity's coordinates to be equal to the given coordinates
		e.SetCoordinates(cc);
		//add entity to save list of they need to be saved
		if (e.ShouldSave)
		{
			instance.toSave.Add(e);
		}
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
				instance.RemoveEntityFromTypeSortedList(e);
				if (e.ShouldSave)
				{
					instance.toSave.Remove(e);
				}
				if (chunk.Count == 0)
				{
					instance.occupiedCoords.Remove(cc);
				}
				return true;
			}
		}

		return false;
	}

	private void AddEntityToTypeSortedList(Entity e)
	{
		Type t = e.GetType();

		if (!entitiesByType.ContainsKey(t))
		{
			entitiesByType.Add(t, new HashSet<Entity>());
		}

		HashSet<Entity> set = entitiesByType[t];
		set.Add(e);
	}

	private void RemoveEntityFromTypeSortedList(Entity e)
	{
		Type t = e.GetType();

		if (!entitiesByType.ContainsKey(t)) return;

		HashSet<Entity> set = entitiesByType[t];
		set.Remove(e);
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
		if (e.GetCoords() == destChunk) return true;

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

	private const string TEMP_SAVE_FILE_NAME = "EntityNetwork_tmp",
		PERMANENT_SAVE_FILE_NAME = "EntityNetwork",
		SAVE_TAG = "EntityNetwork";

	/// <summary>
	/// Grabs all data from a temporary save file and places it into a permanent one.
	/// </summary>
	[MenuItem("Temp/Permanent Save Entity Network")]
	public static void PermanentSave()
	{
		//check if a temporary save file exists
		if (!SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME))
		{
			Debug.LogWarning("No temporary save file exists for the entity network");
			return;
		}
		//copy data from temporary file
		string text = SaveLoad.LoadText(TEMP_SAVE_FILE_NAME);
		//overwrite the permanent file with the copied data
		SaveLoad.SaveText(PERMANENT_SAVE_FILE_NAME, text);
		//delete the temporary file
		SaveLoad.DeleteSaveFile(TEMP_SAVE_FILE_NAME);
		//close the file to prevent issues later
		UnifiedSaveLoad.CloseFile(TEMP_SAVE_FILE_NAME);
	}

	/// <summary>
	/// Grabs data from all saveable entities and puts it all into a temporary file.
	/// </summary>
	[MenuItem("Temp/Temporary Save Entity Network")]
	public static void TempSave()
	{
		//delete existing temporary file
		SaveLoad.DeleteSaveFile(TEMP_SAVE_FILE_NAME);
		//close the file because it was deleted
		UnifiedSaveLoad.CloseFile(TEMP_SAVE_FILE_NAME);
		//reopen the file (which will recreate the file if second argument is true)
		UnifiedSaveLoad.OpenFile(TEMP_SAVE_FILE_NAME, true);
		//create a main tag to put all entities under
		SaveTag mainTag = new SaveTag(SAVE_TAG);
		//loop over entities to save
		int count = 0;
		foreach (Entity e in instance.toSave)
		{
			//ignore if entity is null, remove it from the list too
			if (e == null)
			{
				instance.toSave.Remove(e);
				continue;
			}
			//get the tag string from the entity, the counter ensures each tag is unique
			string tag = $"{e.GetTag()}:{count}";
			//create a nested tag using the main tag as the base
			SaveTag entityTag = new SaveTag(tag, mainTag);
			//get all the data from the entity
			List<DataModule> data = e.GetData();
			//loop over each data module and save it to the temp memory
			foreach (DataModule module in data)
			{
				UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, entityTag, module);
			}
			//increment counter
			count++;
		}

		foreach (Type t in instance.entitiesByType.Keys)
		{
			HashSet<Entity> set = instance.entitiesByType[t];

			foreach (Entity e in set)
			{
				//ignore if entity is null, remove it from the list too
				if (e == null)
				{
					set.Remove(e);
					continue;
				}
				e.Save(TEMP_SAVE_FILE_NAME, mainTag);
				//increment counter
				count++;
			}
		}
		//save the data now in temp memory into a file
		UnifiedSaveLoad.SaveOpenedFile(TEMP_SAVE_FILE_NAME);
	}
}