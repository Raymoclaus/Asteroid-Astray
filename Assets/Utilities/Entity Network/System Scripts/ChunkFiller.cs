using CustomDataTypes;
using System.Collections.Generic;
using UnityEngine;

public class ChunkFiller : MonoBehaviour
{
	private ChunkCoords coords;
	private Vector2 _prevPos = Vector2.positiveInfinity;
	public int FillRange = 2;
	[HideInInspector] public int RangeIncrease;
	private static EntityGenerator _entityGenerator;

	private void Start()
	{
		enabled = false;

		LoadingController loadingController = FindObjectOfType<LoadingController>();
		if (loadingController != null)
		{
			loadingController.OnLoadingComplete.RunWhenReady(Initialise);
		}
	}

	private void Initialise()
	{
		coords = new ChunkCoords(transform.position, EntityNetwork.CHUNK_SIZE);
		//load up some asteroids around the current position
		FillChunks(coords, false);
		enabled = true;
	}

	private void Update()
	{
		CheckForMovement();
	}

	private static EntityGenerator EntityGenerator
	{
		get
		{
			if (_entityGenerator != null && !_entityGenerator.Equals(null)) return _entityGenerator;
			return _entityGenerator = FindObjectOfType<EntityGenerator>();
		}
	}

	public void CheckForMovement()
	{
		Vector2 newPos = transform.position;
		if (_prevPos == newPos) return;
		Moved(newPos);
		_prevPos = newPos;
	}

	private void Moved(Vector2 newPos)
	{
		ChunkCoords newCc = new ChunkCoords(newPos, EntityNetwork.CHUNK_SIZE);
		if (newCc == coords) return;
		CoordsChanged(newCc);
		coords = newCc;
	}

	private void CoordsChanged(ChunkCoords newCc)
	{
		FillChunks(newCc, true);
	}

	private List<ChunkCoords> chunksToFill = new List<ChunkCoords>();
	private void FillChunks(ChunkCoords center, bool batchOrder = false)
	{
		chunksToFill.Clear();
		GetChunkFillList(center, chunksToFill);
		if (!batchOrder)
		{
			EntityGenerator.InstantFillChunks(chunksToFill);
		}
		else
		{
			EntityGenerator.EnqueueBatchOrder(chunksToFill);
		}
	}

	private List<ChunkCoords> GetChunkFillList(ChunkCoords center, List<ChunkCoords> addToList = null)
	{
		int r = FillRange + RangeIncrease;
		int listSize = ((r + 1) * 2) * ((r + 1) * 2);
		List<ChunkCoords> coords = addToList ?? new List<ChunkCoords>(listSize);

		for (int i = -r; i <= r; i++)
		{
			for (int j = -r; j <= r; j++)
			{
				coords.Add(new ChunkCoords(center.quadrant, center.x + i, center.y + j, true));
			}
		}

		return coords;
	}
}