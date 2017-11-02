using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {
	public ChunkCoordinates coords;

	public void Move(Vector2 move) {
		transform.position += (Vector3)move;
		coords = new ChunkCoordinates(transform.position);
	}
}
