using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

/// Keeps track of all entities in an organised network based on their position.
/// Useful in case you only want access to entities in a certain area. It's faster than checking every single one.
public static class EntityNetwork
{
	#region Fields
	//4 directions in the grid
	public const int QuadrantNumber = 4;
	//Determines the physical size of cells in the grid
	public const float CHUNK_SIZE = 10;
	//network of entities
	private static List<List<List<List<Entity>>>> _grid = new List<List<List<List<Entity>>>>(QuadrantNumber);
	//list of coordinates that contain any entities
	private static List<ChunkCoords> occupiedCoords = new List<ChunkCoords>(ReserveSize * ReserveSize * 4);
	//number of chunks to check each frame. Better than checking all at once, for performance
	private const int ENTITY_CHECKUP_NUMBER = 100;
	//some large number of reserve to avoid List resizing lag
	public const int ReserveSize = 1000;
	//another large number for each individual cell in the grid
	public const int CellReserve = 15;
	//check if grid has already been created
	public static bool ready = false;
	//Entities and other stuff that might be create before the grid is initialised can store functions here
	//to be run after the grid is initialised
	public static List<Action> postInitActions = new List<Action>();
	#endregion

	#region Stat Tracking
	private static int numEntities;
	#endregion

	/// Constructs and reserves a large amount of space for the grid
	public static IEnumerator CreateGrid(Action a)
	{
		if (ready) yield break;

		//reserve space in each direction
		//takes ~1.5 seconds for 1000 * 1000 * 10
		for (int dir = 0; dir < QuadrantNumber; dir++)
		{
			_grid.Add(new List<List<List<Entity>>>(ReserveSize));
			for (int x = 0; x < ReserveSize; x++)
			{
				_grid[dir].Add(new List<List<Entity>>(ReserveSize));
				for (int y = 0; y < ReserveSize; y++)
				{
					_grid[dir][x].Add(new List<Entity>(CellReserve));
					//no actual entities are created yet
				}
			}
			yield return null;
		}

		ready = true;
		a?.Invoke();
	}

	public static void RunInitialisationActions()
	{
		for (int i = 0; postInitActions.Count > 0;)
		{
			postInitActions[i]();
			postInitActions.RemoveAt(i);
		}
	}

	/// Returns a list of all entities located in cells within range of the given coordinates
	private static List<ChunkCoords> coordsInRangeCache = new List<ChunkCoords>();
	public static List<Entity> GetEntitiesInRange(ChunkCoords center, int range, EntityType? type = null,
		List<Entity> exclusions = null, List<Entity> addToList = null)
	{
		coordsInRangeCache.Clear();
		List<ChunkCoords> coordsInRange = GetCoordsInRange(center, range, coordsInRangeCache);
		//declare a list to be filled and reserve some room
		return GetEntitiesAtCoords(coordsInRange, type, exclusions, addToList);
	}

	public static List<Entity> GetEntitiesAtCoords(List<ChunkCoords> coordsList, EntityType? type = null,
		List<Entity> exclusions = null, List<Entity> addToList = null)
	{
		List<Entity> entitiesInCoords = addToList ?? new List<Entity>(CellReserve * coordsList.Count);
		EntityType filter = type ?? EntityType.Entity;
		//loop through coordinates list and grab all entities at each coordinate
		foreach (ChunkCoords coord in coordsList)
		{
			if (type == null)
			{
				entitiesInCoords.AddRange(Chunk(coord));
			}
			else
			{
				foreach (Entity e in Chunk(coord))
				{
					if (e.GetEntityType() == filter && !EntityIsInSet(e, exclusions))
					{
						entitiesInCoords.Add(e);
					}
				}
			}
		}
		return entitiesInCoords;
	}

	private static bool EntityIsInSet(Entity e, List<Entity> set)
	{
		if (set == null) return false;

		foreach (Entity entity in set)
		{
			if (entity == e)
			{
				return true;
			}
		}
		return false;
	}

	public static List<ChunkCoords> GetCoordsInRange(ChunkCoords center, int range,
		List<ChunkCoords> coordsInRange = null, bool ignoreLackOfExistence = false)
	{
		int r = range + 2;
		coordsInRange = coordsInRange ?? new List<ChunkCoords>(r * r);
		//loop through surrounding chunks
		for (int i = 0; i <= range; i++)
		{
			GetCoordsOnRangeBorder(center, i, coordsInRange, ignoreLackOfExistence);
		}

		return coordsInRange;
	}

	public static List<ChunkCoords> GetCoordsOnRangeBorder(ChunkCoords center, int range,
		List<ChunkCoords> addToList = null, bool ignoreLackOfExistence = false)
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
				if (ignoreLackOfExistence || ChunkExists(validCc))
				{
					addToList.Add(validCc);
				}
			}
		}

		return addToList;
	}

	public static int CountInRange(ChunkCoords center, int range)
	{
		int count = 0;
		ChunkCoords c = center;
		//loop through surrounding chunks
		for (int i = -range; i <= range; i++)
		{
			c.X = center.X + i;
			for (int j = -range; j <= range; j++)
			{
				c.Y = center.Y + j;
				//validate will adjust for edge cases
				ChunkCoords validCc = c;
				validCc.Validate();
				if (ChunkExists(validCc))
				{
					count += Chunk(validCc).Count;
				}
			}
		}
		return count;
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
		numEntities++;
		//update list of occupied coordinates
		if (Chunk(cc).Count == 1)
		{
			occupiedCoords.Add(cc);
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
				numEntities--;
				if (chunk.Count == 0)
				{
					occupiedCoords.Remove(cc);
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
		for (int dir = 0; dir < _grid.Count; dir++)
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

	/// Removes an entity from its position in the network and replaces it and the given destination
	/// This will mostly be used by entities themselves as they are responsible for determining their place in the
	/// network
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
		return _grid[(int) cc.Direction];
	}

	#endregion

	/// Begins a search and outputs some debug information
	private static ChunkCoords StartFullSearch(Entity e)
	{
		Debug.LogWarning("Starting full search for: " + e);
		ChunkCoords search = ChunkCoords.Zero;
		FunctionTimer timer = new FunctionTimer();
		foreach (List<List<List<Entity>>> dir in _grid)
		{
			foreach (List<List<Entity>> col in dir)
			{
				foreach (List<Entity> chunk in col)
				{
					foreach (Entity ent in chunk)
					{
						if (ent == e)
						{
							return search;
						}
					}
					search.Y++;
				}
				search.X++;
			}
			search.Direction++;
		}
		ChunkCoords result = FullSearch(e, ChunkCoords.Zero);
		Debug.LogWarning("Full search for: " + e + " completed in " + timer.Log() + " seconds.");
		return result;
	}
	
	/// This is slow so keep this for emergencies if you lose an entity
	private static ChunkCoords FullSearch(Entity e, ChunkCoords search)
	{
		if (ConfirmLocation(e, search))
		{
			//if found, return the coordinates the entity was found at
			return search;
		}

		//else adjust the search and try again
		search.Y++;
		if (search.Y >= Column(search).Count)
		{
			search.Y = 0;
			search.X++;
			if (search.X >= Direction(search).Count)
			{
				search.X = 0;
				search.Direction++;
				//if every element has been checked, return invalid coordinates
				if ((int) search.Direction >= _grid.Count)
				{
					Debug.LogWarning("Entity not found in grid.");
					return ChunkCoords.Invalid;
				}
			}
		}

		return FullSearch(e, search);
	}

	/// Returns whether a given chunk is valid and exists in the network
	public static bool ChunkExists(ChunkCoords cc)
	{
		//if coordinates are invalid then chunk definitely doesn't exist
		if (!cc.IsValid())
		{
			return false;
		}

		//if the quadrant doesn't have x amount of columns or that column doesn't have y amount of cells,
		//the chunk doesn't exist
		if ((int) cc.Direction >= _grid.Count
		    || cc.X >= Direction(cc).Count
		    || cc.Y >= Column(cc).Count)
		{
			return false;
		}

		return true;
	}

	private static ChunkCoords Next(ChunkCoords c)
	{
		if (c.X < Column(c).Count - 1)
		{
			c.X += 1;
			return c;
		}

		c.X = 0;
		if (c.Y < Direction(c).Count - 1)
		{
			c.Y += 1;
			return c;
		}

		c.Y = 0;
		if ((int)c.Direction < _grid.Count - 1)
		{
			c.Direction += 1;
			return c;
		}

		c.Direction = 0;
		return c;
	}

	public static bool ConfirmLocation(Entity e, ChunkCoords c)
	{
		foreach (Entity ent in Chunk(c))
		{
			if (ent == e)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsType(EntityType type, ChunkCoords c, Entity entToExclude)
	{
		if (!ChunkExists(c)) return false;

		foreach (Entity e in Chunk(c))
		{
			if (e.GetEntityType() == type && e != entToExclude)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetEntityCount()
	{
		return numEntities;
	}
}