using CustomDataTypes;
using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using SaveType = SaveSystem.SaveType;

/// Keeps track of all entities in an organised network based on their position.
/// Allows entities to be placed pseudo-infinitely in any 2D direction, although it can be upgraded to include 3D.
/// Useful in case you only want access to entities in a certain area. It's faster than checking every single one.
/// Intended for use with procedurally-generated content.
public class EntityNetwork : MonoBehaviour
{
	private static int QuadrantCount
		=> ChunkCoords.DIRECTION_COUNT;
	//Determines the physical size of cells in the grid
	public const float CHUNK_SIZE = 10f;
	//network of entities
	private List<List<List<List<Entity>>>> grid = new List<List<List<List<Entity>>>>(QuadrantCount);
	//list of coordinates that contain any entities
	private HashSet<ChunkCoords> occupiedCoords = new HashSet<ChunkCoords>();
	//dictionary by type, containing dictionary by coordinates
	private Dictionary<Type, Dictionary<ChunkCoords, HashSet<Entity>>> entitiesByTypePerCoordinates = new Dictionary<Type, Dictionary<ChunkCoords, HashSet<Entity>>>();
	private EntityGenerator _entityGenerator;

	public InvocableOneShotEvent OnLoaded = new InvocableOneShotEvent();

	private void Awake()
	{
		OneShotEventGroupWait wait = new OneShotEventGroupWait(false,
			UniqueIDGenerator.OnLoaded);

		_entityGenerator = FindObjectOfType<EntityGenerator>();
		if (_entityGenerator != null)
		{
			wait.AddEventToWaitFor(_entityGenerator.OnPrefabsLoaded);
		}

		wait.Start();
		wait.RunWhenReady(Load);
	}

	public bool IsReady => OnLoaded.Invoked;

	/// Returns a list of all entities located in cells within range of the given coordinates
	public bool IterateEntitiesInRange(ChunkCoords center, int range, Func<Entity, bool> action)
	{
		return IterateCoordsInRange(
			center,
			range,
			cc =>
			{
				return IterateEntitiesAtCoord(cc, action);
			});
	}

	public bool IterateEntitiesAtCoord(ChunkCoords coord, Func<Entity, bool> action)
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
	private bool EntityIsInSet(Entity e, List<Entity> set)
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
		Func<ChunkCoords, bool> action)
	{
		int r = range + 2;
		//loop through surrounding chunks
		for (int i = 0; i <= range; i++)
		{
			if (IterateCoordsOnRangeBorder(center, i, action))
			{
				return true;
			}
		}

		return false;
	}

	/// Returns a list of coordinates that are a specified distance from a given center coordinate
	public static bool IterateCoordsOnRangeBorder(ChunkCoords center, int range,
		Func<ChunkCoords, bool> exitCondition)
	{
		int r = range * 8;

		ChunkCoords pos = new ChunkCoords(IntPair.zero, CHUNK_SIZE);
		for (pos.x = -range; pos.x <= range;)
		{
			for (pos.y = -range; pos.y <= range;)
			{
				ChunkCoords validCC = (center + pos).Validate();
				if (exitCondition?.Invoke(validCC) ?? false) return true;
				pos.y += pos.x <= -range || pos.x >= range ?
					1 : range * 2;
			}
			pos.x += pos.y <= -range || pos.y >= range ?
				1 : range * 2;
		}

		return false;
	}

	/// Adds a given entity to the list at given coordinates
	public bool AddEntity(Entity e, ChunkCoords cc)
	{
		//check if the given coordinates are valid
		if (!cc.IsValid())
		{
			Debug.LogWarning("Coordinates to add entity to are invalid.");
			return false;
		}
		//make sure that the chunk with those coordinates exists
		EnsureChunkExists(cc);
		//add entity to that chunk
		Chunk(cc).Add(e);
		//set entity's coordinates to be equal to the given coordinates
		e.SetCoordinates(cc);
		//add entity to type-sorted list
		AddEntityToTypeSortedList(e);
		//update list of occupied coordinates
		if (Chunk(cc).Count == 1)
		{
			occupiedCoords.Add(cc);
		}
		return true;
	}

	/// Removes an entity from the network
	public bool RemoveEntity(Entity e, EntityType? type = null)
	{
		ChunkCoords cc = e.GetCoords();
		if (!ChunkExists(cc)) return false;

		List<Entity> chunk = Chunk(cc);
		for (int i = 0; i < chunk.Count; i++)
		{
			if (chunk[i] == e)
			{
				chunk.RemoveAt(i);
				RemoveEntityFromTypeSortedList(e);
				if (chunk.Count == 0)
				{
					occupiedCoords.Remove(cc);
				}
				return true;
			}
		}

		return false;
	}

	private void AddEntityToTypeSortedList(Entity e)
	{
		Type t = e.GetType();
		ChunkCoords cc = e.GetCoords();

		if (!entitiesByTypePerCoordinates.ContainsKey(t))
		{
			entitiesByTypePerCoordinates.Add(t, new Dictionary<ChunkCoords, HashSet<Entity>>());
		}

		if (!entitiesByTypePerCoordinates[t].ContainsKey(cc))
		{
			entitiesByTypePerCoordinates[t].Add(cc, new HashSet<Entity>());
		}

		HashSet<Entity> set = entitiesByTypePerCoordinates[t][cc];
		set.Add(e);
	}

	private void RemoveEntityFromTypeSortedList(Entity e)
	{
		Type t = e.GetType();
		ChunkCoords cc = e.GetCoords();

		if (!entitiesByTypePerCoordinates.ContainsKey(t)) return;
		if (!entitiesByTypePerCoordinates[t].ContainsKey(cc)) return;

		HashSet<Entity> set = entitiesByTypePerCoordinates[t][cc];
		set.Remove(e);

		if (set.Count == 0) entitiesByTypePerCoordinates[t].Remove(cc);
	}

	/// Iterates through all entities and performs the action once for each entity
	public void AccessAllEntities(Action<Entity> act, EntityType? onlyType = null)
	{
		ChunkCoords check = ChunkCoords.Zero;
		bool limitCheck = onlyType != null;
		//Check every direction
		for (int dir = 0; dir < grid.Count; dir++)
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
						if ((limitCheck && e.EntityType== (EntityType) onlyType) || !limitCheck)
						{
							act(e);
						}
					}
				}
			}
		}
	}

	public void DestroyAllEntities()
		=> AccessAllEntities((Entity e) => e.DestroySelf(null, 0f));

	/// Removes an entity from its position in the network and replaces it and the given destination
	/// This will mostly be used by entities themselves as they are responsible for determining their place in the network
	public bool Reposition(Entity e, ChunkCoords destChunk)
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

	public List<Entity> Chunk(ChunkCoords cc) => Column(cc)[cc.y];

	public List<List<Entity>> Column(ChunkCoords cc) => Quad(cc)[cc.x];

	public List<List<List<Entity>>> Quad(ChunkCoords cc)
		=> grid[(int)cc.quadrant];

	/// Returns whether a given chunk is valid and exists in the network
	private bool ChunkExists(ChunkCoords cc)
	{
		//if coordinates are invalid then chunk definitely doesn't exist
		if (!cc.IsValid() || !IsReady) return false;

		//if the quadrant doesn't have x amount of columns or that column doesn't have y amount of cells,
		//the chunk doesn't exist
		if ((int) cc.quadrant >= grid.Count
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
	public bool ConfirmLocation(Entity e, ChunkCoords c)
	{
		return EntityIsInSet(e, Chunk(c));
	}

	/// Returns whether a chunk contains any entities of a certain type
	public bool ContainsType(EntityType type, ChunkCoords c, Entity entToExclude = null)
	{
		if (!ChunkExists(c)) return false;

		for (int i = 0; i < Chunk(c).Count; i++)
		{
			Entity e = Chunk(c)[i];
			if (e.EntityType== type && e != entToExclude) return true;
		}
		return false;
	}

	private const string TEMP_SAVE_FILE_NAME = "EntityNetwork_tmp",
		PERMANENT_SAVE_FILE_NAME = "EntityNetwork",
		SAVE_TAG_NAME = "EntityNetwork",
		ENTITIES_TAG_NAME = "Entities";

	/// <summary>
	/// Grabs data from all saveable entities and puts it all into a temporary file.
	/// </summary>
	public void TemporarySave()
	{
		//delete existing temporary file
		DeleteTemporarySave();
		//reopen the file (which will recreate the file if second argument is true)
		UnifiedSaveLoad.OpenFile(TEMP_SAVE_FILE_NAME, true);
		//create a main tag
		SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
		//create a tag to put all entities under
		SaveTag entityTag = new SaveTag(ENTITIES_TAG_NAME, mainTag);
		//loop over entities to save fully
		foreach (Type t in entitiesByTypePerCoordinates.Keys)
		{
			Dictionary<ChunkCoords, HashSet<Entity>> chunkSet = entitiesByTypePerCoordinates[t];
			bool skip = false;
			bool amountOnly = false;
			foreach (ChunkCoords cc in chunkSet.Keys)
			{
				HashSet<Entity> entitySet = chunkSet[cc];
				if (!amountOnly) //full save
				{
					foreach (Entity e in entitySet)
					{
						if (e.SaveType == SaveType.OnlyAmount)
						{
							amountOnly = true;
							break;
						}
						else if (e.SaveType == SaveType.NoSave)
						{
							skip = true;
							break;
						}
						//ignore if entity is null, remove it from the list too
						if (e == null)
						{
							entitySet.Remove(e);
							continue;
						}
						//tell the entity to save itself to the given file under the given tag
						e.Save(TEMP_SAVE_FILE_NAME, entityTag);
					}
				}

				if (amountOnly)
				{
					SaveTag chunkTag = new SaveTag($"Chunk:{cc}", entityTag);
					DataModule module = new DataModule($"{t.Name}:Count", entitySet.Count);
					UnifiedSaveLoad.UpdateOpenedFile(TEMP_SAVE_FILE_NAME, chunkTag, module);
				}

				if (skip) break;
			}
		}

		//save the data now in temp memory into a file
		UnifiedSaveLoad.SaveOpenedFile(TEMP_SAVE_FILE_NAME);
	}

	/// <summary>
	/// Grabs all data from a temporary save file and places it into a permanent one.
	/// </summary>
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
		DeleteTemporarySave();
	}

	public static void DeleteTemporarySave()
	{
		//delete existing temporary file
		SaveLoad.DeleteSaveFile(TEMP_SAVE_FILE_NAME);
		//close the file because it was deleted
		UnifiedSaveLoad.CloseFile(TEMP_SAVE_FILE_NAME);
	}

	public void Load()
	{
		Debug.Log("Entity Network Data: Begin Loading");
		//check if save file exists
		bool tempSaveExists = SaveLoad.RelativeSaveFileExists(TEMP_SAVE_FILE_NAME);
		bool permanentSaveExists = SaveLoad.RelativeSaveFileExists(PERMANENT_SAVE_FILE_NAME);
		string filename = null;
		if (tempSaveExists)
		{
			filename = TEMP_SAVE_FILE_NAME;
		}
		else if (permanentSaveExists)
		{
			filename = PERMANENT_SAVE_FILE_NAME;
		}
		else
		{
			Debug.Log("Entity Network Data: Nothing to load, continuing");
			OnLoaded.Invoke();
			return;
		}

		//open the save file
		UnifiedSaveLoad.OpenFile(filename, false);
		//create save tag
		SaveTag mainTag = new SaveTag(SAVE_TAG_NAME);
		//create sub-tag that entities are placed under
		SaveTag entityTag = new SaveTag(ENTITIES_TAG_NAME, mainTag);
		//iterate over entity tag
		UnifiedSaveLoad.IterateTagContents(
			filename,
			entityTag,
			parameterCallBack: module => ApplyData(module),
			subtagCallBack: subtag => CheckSubtag(filename, subtag));

		Debug.Log("Entity Network Data: Loaded");
		OnLoaded.Invoke();
	}

	private bool ApplyData(DataModule module)
	{
		switch (module.parameterName)
		{
			default:
				return false;
		}

		return true;
	}

	private bool CheckSubtag(string filename, SaveTag subtag)
	{
		Entity.LoadEntityFromFile(filename, subtag);
		return true;
	}
}