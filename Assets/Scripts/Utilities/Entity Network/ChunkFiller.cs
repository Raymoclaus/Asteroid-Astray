using UnityEngine;
using System.Collections.Generic;

public class ChunkFiller : MonoBehaviour
{
	private ChunkCoords _coords;
	private Vector2 _prevPos = Vector2.positiveInfinity;
	public int FillRange = 2;
	[HideInInspector] public int RangeIncrease;
	private bool ready = false;

	private void Start()
	{
		enabled = false;
		EntityNetwork.AddListener(() =>
		{
			Initialise();
			enabled = true;
		});
	}

	private void Initialise()
	{
		_coords = new ChunkCoords(transform.position);
		//load up some asteroids around the current position
		FillChunks(_coords, false);
		ready = true;
	}

	private void Update()
	{
		if (!ready) return;

		CheckForMovement();
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
		ChunkCoords newCc = new ChunkCoords(newPos);
		if (newCc == _coords) return;
		CoordsChanged(newCc);
		_coords = newCc;
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
				coords.Add(new ChunkCoords(center.Direction, center.x + i, center.y + j, true));
			}
		}

		return coords;
	}
}