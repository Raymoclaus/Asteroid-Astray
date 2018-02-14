using UnityEngine;

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
		FillChunks(_coords);
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
		FillChunks(newCc);
	}

	private void FillChunks(ChunkCoords center)
	{
		int range = FillRange + RangeIncrease;
		for (int i = -range; i <= range; i++)
		{
			for (int j = -range; j <= range; j++)
			{
				ChunkCoords check = new ChunkCoords(center.Direction, center.X + i, center.Y + j, true);
				AsteroidGenerator.FillChunk(check);
			}
		}
	}
}