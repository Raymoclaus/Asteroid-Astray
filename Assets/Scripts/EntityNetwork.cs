using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityNetwork {
	//4 directions in the grid
	public const int QUADRANT_NUMBER = 4;
	//network of entities
	public static List<List<List<List<Entity>>>> grid = new List<List<List<List<Entity>>>>(QUADRANT_NUMBER);
	//some large number of reserve to avoid List resizing lag
	public static int reserveSize = 1000;
	//another large number for each individual cell in the grid
	public static int cellReserve = 50;

	public static void CreateGrid() {
		//reserve space in each direction
		for (int dir = 0; dir < QUADRANT_NUMBER; dir++) {
			grid.Add(new List<List<List<Entity>>>(reserveSize));
			for (int x = 0; x < reserveSize; x++) {
				grid[dir].Add(new List<List<Entity>>(reserveSize));
				for (int y = 0; y < reserveSize; y++) {
					grid[dir][x].Add(new List<Entity>(cellReserve));
					//no actual entities are created yet
				}
			}
		}
	}

	///Returns a list of all entities located in cells within range of the given coordinates
	public static List<Entity> GetEntitiesInRange(ChunkCoordinates center, int range) {
		//declare a list to be filled and reserve some room
		List<Entity> entitiesInRange = new List<Entity>(cellReserve * (int)Mathf.Pow(range * 2 + 1, 2));
		ChunkCoordinates cc = center;
		for (int i = -range; i < range; i++) {
			cc.x = center.x + i;
			for (int j = -range; j < range; j++) {
				cc.y = center.y + j;
				cc.Validate();
				entitiesInRange.AddRange(grid[(int)cc.direction][cc.x][cc.y]);
			}
		}
		return entitiesInRange;
	}
}
