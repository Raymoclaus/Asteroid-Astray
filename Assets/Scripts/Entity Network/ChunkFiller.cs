﻿using UnityEngine;
using System.Collections.Generic;

public class ChunkFiller : MonoBehaviour
{
	private ChunkCoords _coords;
	private Vector2 _prevPos = Vector2.positiveInfinity;
	public int FillRange = 2;
	[HideInInspector] public int RangeIncrease;

	private void Start()
	{
		_coords = new ChunkCoords(transform.position);
		//load up some asteroids around the current position
		FillChunks(_coords, false);
	}

	private void Update()
	{
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

	private void FillChunks(ChunkCoords center, bool? batchOrder = null)
	{
		List<ChunkCoords> coords = GetChunkFillList(center);
		if (batchOrder == null || !(bool)batchOrder)
		{
			AsteroidGenerator.InstantFillChunks(coords);
		}
		else
		{
			StartCoroutine(AsteroidGenerator.ChunkBatchOrder(coords));
		}
	}

	private List<ChunkCoords> GetChunkFillList(ChunkCoords center)
	{
		List<ChunkCoords> coords = new List<ChunkCoords>(100);
		int r = FillRange + RangeIncrease;

		for (int i = -r; i <= r; i++)
		{
			for (int j = -r; j <= r; j++)
			{
				coords.Add(new ChunkCoords(center.Direction, center.X + i, center.Y + j, true));
			}
		}

		return coords;
	}
}