using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OPool {
	//references to all created asteroids sorted by [direction][chunkX][chunkY][asteroids in a chunk]
	//direction is either upLeft/upRight/downLeft/downRight
	public static List<List<List<List<AsteroidCtrl>>>> asteroids = new List<List<List<List<AsteroidCtrl>>>>();

	//return a list of objects within range of specified coordinates
	public static List<GameObject> GetChunks(ChunkCoordinates crds, int range) {
		List<GameObject> objs = new List<GameObject>();
		ChunkCoordinates checkCrds = crds;
		for (int i = -range; i < range; i++) {
			for (int j = -range; j < range; j++) {
				checkCrds.x = crds.x + i;
				checkCrds.y = crds.y + j;
				objs.AddRange(GetObjsInChunk(checkCrds));
			}
		}

		return objs;
	}

	//returns a list of objects at specified coordinates
	public static List<GameObject> GetObjsInChunk(ChunkCoordinates crds) {
		List<GameObject> objs = new List<GameObject>();
		crds = crds.Validate();
		foreach (AsteroidCtrl obj in asteroids[crds.direction][crds.x][crds.y]) {
			objs.Add(obj.gameObject);
		}
		return objs;
	}
}
