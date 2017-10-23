using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGenerator : MonoBehaviour {
	/* Fields */

	#region

	//reference to asteroid prefab
	public AsteroidCtrl asteroid;
	//keeps track of whether chunks have been filled already. Prevents chunk from refilling if emptied by player
	private List<List<List<bool>>> wasFilled = new List<List<List<bool>>>();
	//how many asteroids per unit. eg a value of 0.3f means: number of asteroids per chunk = 0.3f * CHUNK_SIZE^2
	public float asteroidDensity;

	#endregion

	void Start() {
		CreateDirections();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.RightBracket)) {
			Debug.Log(string.Format("Number of chunks created: {0}", GetNumberOfChunks(false)));
			Debug.Log(string.Format("Number of chunks filled: {0}", GetNumberOfChunks(true)));
			Debug.Log(string.Format("Number of asteroids existing: {0}", GetAsteroidCount()));
		}
	}

	/* Stuff that gets statistics about asteroids. Not important for gameplay. */

	#region

	private int GetNumberOfChunks(bool checkingFilled) {
		int count = 0;
		for (int dir = 0; dir < OPool.asteroids.Count; dir++) {
			for (int x = 0; x < OPool.asteroids[dir].Count; x++) {
				if (checkingFilled) {
					for (int y = 0; y < OPool.asteroids[dir][x].Count; y++) {
						if (wasFilled[dir][x][y]) {
							count++;
						}
					}
				} else {
					count += OPool.asteroids[dir][x].Count;
				}
			}
		}
		return count;
	}

	private int GetAsteroidCount() {
		int count = 0;
		for (int dir = 0; dir < OPool.asteroids.Count; dir++) {
			for (int x = 0; x < OPool.asteroids[dir].Count; x++) {
				for (int y = 0; y < OPool.asteroids[dir][x].Count; y++) {
					count += OPool.asteroids[dir][x][y].Count;
				}
			}
		}
		return count;
	}

	#endregion

	public void FillChunk(ChunkCoordinates chCoord) {
		//if these coordinates have no been generated yet then reserve some space for the new coordinates
		GenerateVoid(chCoord);
		//if these coordinates haven't been filled yet then carry out filling process
		if (!wasFilled[chCoord.direction][chCoord.x][chCoord.y]) {
			//flag that this chunk coordinates was filled
			wasFilled[chCoord.direction][chCoord.x][chCoord.y] = true;
			//fill chunk with asteroids
			Vector2[] range = ChunkCoordinates.GetRange(chCoord);
			Vector2 spawnPos = new Vector2();
			for (int i = 0; i < (int)(Mathf.Pow(Cnsts.CHUNK_SIZE, 2f) * asteroidDensity); i++) {
				//pick a position within the chunk coordinates
				spawnPos.x = Random.Range(range[0].x, range[1].x);
				spawnPos.y = Random.Range(range[0].y, range[1].y);
				//spawn asteroid at coordinates
				AsteroidCtrl newAsteroid = Instantiate<AsteroidCtrl>(
					                           asteroid, spawnPos, Quaternion.identity, transform);
				OPool.asteroids[chCoord.direction][chCoord.x][chCoord.y].Add(newAsteroid);
				newAsteroid.ChunkRefInit(chCoord, i, this);
			}
		}
	}

	private void GenerateVoid(ChunkCoordinates chCoord) {
		if (OPool.asteroids.Count < 4) {
			CreateDirections();
		}
		//check to see if the coordinate has been previously recorded
		while (OPool.asteroids[chCoord.direction].Count <= chCoord.x) {
			//add x coordinates until chCoord is within limits
			OPool.asteroids[chCoord.direction].Add(new List<List<AsteroidCtrl>>());
			wasFilled[chCoord.direction].Add(new List<bool>());
		}
		while (OPool.asteroids[chCoord.direction][chCoord.x].Count <= chCoord.y) {
			//add y coordinates until chCoord is within limits
			OPool.asteroids[chCoord.direction][chCoord.x].Add(new List<AsteroidCtrl>());
			wasFilled[chCoord.direction][chCoord.x].Add(false);
		}
	}

	private void CreateDirections() {
		//create all 4 directions
		int directions = 4 - OPool.asteroids.Count;
		for (int i = 0; i < directions; i++) {
			OPool.asteroids.Add(new List<List<List<AsteroidCtrl>>>());
			wasFilled.Add(new List<List<bool>>());
		}
	}

	public void DestroyAll() {
		//destroy all asteroids and clear the array
		for (int dir = 0; dir < OPool.asteroids.Count; dir++) {
			for (int x = 0; x < OPool.asteroids[dir].Count; x++) {
				for (int y = 0; y < OPool.asteroids[dir][x].Count; y++) {
					for (int i = 0; i < OPool.asteroids[dir][x][y].Count; i++) {
						Destroy(OPool.asteroids[dir][x][y][i]);
					}
				}
			}
		}
		//clear all arrays
		OPool.asteroids.Clear();
		wasFilled.Clear();
		//refill directions
		CreateDirections();
	}

	public void DestroyAsteroid(ChunkCoordinates refVal, int part) {
		OPool.asteroids[refVal.direction][refVal.x][refVal.y].RemoveAt(part);
		DecrementAsteroidIds(refVal, part);
	}

	private void DecrementAsteroidIds(ChunkCoordinates chunk, int part) {
		for (int i = part; i < OPool.asteroids[chunk.direction][chunk.x][chunk.y].Count; i++) {
			OPool.asteroids[chunk.direction][chunk.x][chunk.y][i].chunkPart--;
		}
	}
}
