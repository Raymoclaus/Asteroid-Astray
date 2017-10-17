using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkCoordinates
{
	public int direction, x, y;

	public ChunkCoordinates(Vector2 pos)
	{
		this.direction = GetDirection(pos);
		int[] coord = ConvertToXY(pos);
		this.x = coord[0];
		this.y = coord[1];
	}

	private static int[] ConvertToXY(Vector2 pos)
	{
		pos /= Cnsts.CHUNK_SIZE;
		return new int[] { (int)Mathf.Abs(pos.x), (int)Mathf.Abs(pos.y) };
	}

	public static int GetDirection(Vector2 pos)
	{
		pos /= Cnsts.CHUNK_SIZE;
		return GetDirection(pos.x, pos.y);
	}
	public static int GetDirection(float x, float y)
	{
		int direction = 0;
		if (x >= 0)
		{
			if (y >= 0)
			{
				direction = 1;
			}
			else
			{
				direction = 3;
			}
		}
		else
		{
			if (y >= 0)
			{
				direction = 0;
			}
			else
			{
				direction = 2;
			}
		}
		return direction;
	}

	public static Vector2[] GetRange(ChunkCoordinates chCoord)
	{
		Vector2 min = new Vector2(chCoord.x, chCoord.y);
		Vector2 max = new Vector2(chCoord.x + 1, chCoord.y + 1);
		switch(chCoord.direction)
		{
		case 0:
			min.x *= -1f;
			max.x *= -1f;
//			min.x -= 1f;
//			max.x += 1f;
			break;
		case 2:
			min *= -1F;
			max *= -1F;
//			min -= Vector2.one;
//			max -= Vector2.one;
			break;
		case 3:
			min.y *= -1f;
			max.y *= -1f;
//			min.y -= 1f;
//			max.y += 1f;
			break;
		}
		return new Vector2[] { min * Cnsts.CHUNK_SIZE, max * Cnsts.CHUNK_SIZE };
	}

	public override string ToString()
	{
		return string.Format(string.Format("Direction: {0}, Coordinates({1}, {2})", direction, x, y));
	}
}
