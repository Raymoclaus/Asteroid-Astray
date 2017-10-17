using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkFiller : MonoBehaviour
{
	public AsteroidGenerator asterGen;
	private Vector3 prevPos;

	void Start()
	{
		if (asterGen == null)
		{
			asterGen = FindObjectOfType<AsteroidGenerator>();
		}
		//fill the chunk that the object is currently in
		asterGen.FillChunk(new ChunkCoordinates((Vector2)transform.position));
		//fill chunks around the current chunk
		FillChunks();
	}

	void Update()
	{
		if (prevPos != transform.position)
		{
			Moved();
			prevPos = transform.position;
		}
	}

	private void Moved()
	{
		FillChunks();
	}

	private void FillChunks()
	{
		//attempt to fill chunks around the object
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + Vector2.up * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + (Vector2.right + Vector2.up) * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + Vector2.right * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + (Vector2.right + Vector2.down) * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + Vector2.down * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + (Vector2.left + Vector2.down) * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + Vector2.left * Cnsts.CHUNK_SIZE * 0.9f));
		asterGen.FillChunk(new ChunkCoordinates(
			(Vector2)transform.position + (Vector2.left + Vector2.up) * Cnsts.CHUNK_SIZE * 0.9f));
	}
}
