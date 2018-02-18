using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AsteroidGenerator
{
	#region Fields
	//reference to asteroid prefab
	public static Asteroid AsteroidPfb;
	//reference to the holder of asteroid objects
	public static Transform AsteroidHolder;
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private static List<List<List<bool>>> _wasFilled = new List<List<List<bool>>>();
	//how many asteroids per unit. eg a value of 0.3f means: number of asteroids per chunk = 0.3f * CHUNK_SIZE^2
	public static float AsteroidDensity = 0.1f;
	//chunks to fill in batches
	private static List<ChunkCoords> chunkBatches = new List<ChunkCoords>(100);
	#endregion

	public static void FillChunk(ChunkCoords cc)
	{
		//don't bother if the given coordinates are not valid
		if (!cc.IsValid()) return;

		//if these coordinates have no been generated yet then reserve some space for the new coordinates
		GenerateVoid(cc);
		//don't bother if the coordinates have already been filled
		if (Chunk(cc)) return;

		//flag that this chunk coordinates was filled
		Column(cc)[cc.Y] = true;
		//fill chunk with asteroids
		Vector2Pair range = ChunkCoords.GetCellArea(cc);
		Vector2 spawnPos = new Vector2();
		for (int i = 0; i < (int) (Cnsts.CHUNK_SIZE * Cnsts.CHUNK_SIZE * AsteroidDensity); i++)
		{
			//pick a position within the chunk coordinates
			spawnPos.x = Random.Range(range.A.x, range.B.x);
			spawnPos.y = Random.Range(range.A.y, range.B.y);
			//spawn asteroid at coordinates
			Object.Instantiate(AsteroidPfb, spawnPos, Quaternion.identity, AsteroidHolder);
		}
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
		while (chunkBatches.Count > 0)
		{
			for (int i = 0; i < Mathf.Round(Cnsts.TIME_SPEED) && chunkBatches.Count > 0; i++)
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
			Debug.Log("Row capacity breached.");
			Direction(cc).Capacity = cc.X + 1;
		}

		while (Direction(cc).Count <= cc.X)
		{
			Direction(cc).Add(new List<bool>());
		}

		//add more rows until the column is large enough to make the given coordinates valid
		if (Column(cc).Capacity <= cc.Y)
		{
			Debug.Log("Column capacity breached.");
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

	/* Shorthand methods for accessing the grid */

	#region

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