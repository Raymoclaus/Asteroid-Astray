using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Quadrant { UpperLeft, UpperRight, LowerLeft, LowerRight }

public struct Vector2Pair {
	public Vector2 a, b;
	public Vector2Pair(Vector2 a, Vector2 b) {
		this.a = a;
		this.b = b;
	}
}

public struct ChunkCoordinates
{
	public Quadrant direction;
	public int x, y;

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

	public static Quadrant GetDirection(Vector2 pos)
	{
		pos /= Cnsts.CHUNK_SIZE;
		return GetDirection(pos.x, pos.y);
	}
	public static Quadrant GetDirection(float x, float y)
	{
		Quadrant dir;
		if (x >= 0)
		{
			if (y >= 0)
			{
				dir = Quadrant.UpperRight;
			}
			else
			{
				dir = Quadrant.LowerRight;
			}
		}
		else
		{
			if (y >= 0)
			{
				dir = Quadrant.UpperLeft;
			}
			else
			{
				dir = Quadrant.LowerLeft;
			}
		}
		return dir;
	}

	public static Vector2Pair GetCellArea(ChunkCoordinates chCoord)
	{
		Vector2 min = new Vector2(chCoord.x, chCoord.y);
		Vector2 max = new Vector2(chCoord.x + 1, chCoord.y + 1);
		switch(chCoord.direction)
		{
			case Quadrant.UpperLeft:
			min.x *= -1f;
			max.x *= -1f;
			break;
			case Quadrant.LowerLeft:
			min *= -1F;
			max *= -1F;
			break;
			case Quadrant.LowerRight:
			min.y *= -1f;
			max.y *= -1f;
			break;
		}
		return new Vector2Pair(min * Cnsts.CHUNK_SIZE, max * Cnsts.CHUNK_SIZE);
	}

	public bool IsValid() {
		return (int)direction >= 0 && (int)direction <= 3 && x >= 0 && y >= 0;
	}

	public ChunkCoordinates Validate() {
		if (!IsValid()) {
			//fix direction to be within bounds
			direction = (Quadrant)(Mathf.Abs((int)direction) % 4);
			//adjust direction if x is not valid
			if (x < 0) {
				switch (direction) {
					case Quadrant.UpperLeft:
						direction = Quadrant.UpperRight;
						break;
					case Quadrant.UpperRight:
						direction = Quadrant.UpperLeft;
						break;
					case Quadrant.LowerLeft:
						direction = Quadrant.LowerRight;
						break;
					case Quadrant.LowerRight:
						direction = Quadrant.LowerLeft;
						break;
				}
				x = Mathf.Abs(x) - 1;
			}
			//adjust direction if y is not valid
			if (y < 0) {
				switch (direction) {
					case Quadrant.UpperLeft:
						direction = Quadrant.LowerLeft;
						break;
					case Quadrant.UpperRight:
						direction = Quadrant.LowerRight;
						break;
					case Quadrant.LowerLeft:
						direction = Quadrant.UpperLeft;
						break;
					case Quadrant.LowerRight:
						direction = Quadrant.UpperRight;
						break;
				}
			}
		}
		return this;
	}

	public override string ToString()
	{
		return string.Format(string.Format("Direction: {0}, Coordinates({1}, {2})", direction, x, y));
	}
}
