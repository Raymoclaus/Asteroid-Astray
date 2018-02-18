using System;
using UnityEngine;

public enum Quadrant
{
	UpperLeft,
	UpperRight,
	LowerLeft,
	LowerRight
}

public struct Vector2Pair
{
	public Vector2 A, B;

	public Vector2Pair(Vector2 a, Vector2 b)
	{
		A = a;
		B = b;
	}
}

public struct IntPair
{
	public int A, B;

	public IntPair(int a, int b)
	{
		A = a;
		B = b;
	}
}

public struct ChunkCoords
{
	public Quadrant Direction;
	public int X, Y;

	public static ChunkCoords Invalid
	{
		get { return new ChunkCoords((Quadrant) (-1), -1, -1); }
	}

	public static ChunkCoords Zero
	{
		get { return new ChunkCoords(Quadrant.UpperLeft, 0, 0); }
	}

	public ChunkCoords(Vector2 pos)
	{
		this = PosToCoords(pos);
	}

	public ChunkCoords(Quadrant direction, int x, int y, bool? shouldValidate = null)
	{
		Direction = direction;
		X = x;
		Y = y;

		if (shouldValidate != null && (bool) shouldValidate)
		{
			this = Validate();
		}
	}

	public static ChunkCoords PosToCoords(Vector2 pos)
	{
		ChunkCoords cc;
		cc.Direction = GetDirection(pos);
		IntPair coord = ConvertToXy(pos);
		cc.X = coord.A;
		cc.Y = coord.B;
		return cc;
	}

	private static IntPair ConvertToXy(Vector2 pos)
	{
		pos /= Cnsts.CHUNK_SIZE;
		return new IntPair(Math.Abs((int) pos.x), Math.Abs((int) pos.y));
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

	public static Vector2Pair GetCellArea(ChunkCoords chCoord)
	{
		Vector2 min = new Vector2(chCoord.X, chCoord.Y);
		Vector2 max = new Vector2(chCoord.X + 1, chCoord.Y + 1);
		switch (chCoord.Direction)
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

	public bool IsValid()
	{
		return this != Invalid
		       && (int) Direction >= 0
		       && (int) Direction < EntityNetwork.QuadrantNumber
		       && X >= 0
		       && Y >= 0;
	}

	public ChunkCoords Validate()
	{
		//if it is already valid then no changes required
		if (IsValid())
		{
			return this;
		}

		//fix direction to be within bounds
		Direction = (Quadrant) (Math.Abs((int) Direction) % EntityNetwork.QuadrantNumber);
		//adjust direction if x is not valid
		if (X < 0)
		{
			switch (Direction)
			{
				case Quadrant.UpperLeft:
					Direction = Quadrant.UpperRight;
					break;
				case Quadrant.UpperRight:
					Direction = Quadrant.UpperLeft;
					break;
				case Quadrant.LowerLeft:
					Direction = Quadrant.LowerRight;
					break;
				case Quadrant.LowerRight:
					Direction = Quadrant.LowerLeft;
					break;
			}

			X = Math.Abs(X) - 1;
		}

		//adjust direction if y is not valid
		if (Y < 0)
		{
			switch (Direction)
			{
				case Quadrant.UpperLeft:
					Direction = Quadrant.LowerLeft;
					break;
				case Quadrant.UpperRight:
					Direction = Quadrant.LowerRight;
					break;
				case Quadrant.LowerLeft:
					Direction = Quadrant.UpperLeft;
					break;
				case Quadrant.LowerRight:
					Direction = Quadrant.UpperRight;
					break;
			}

			Y = Math.Abs(Y) - 1;
		}

		return this;
	}

	/// Returns the distance between two coordinates. (Diagonal distance is treated the same as axis distance)
	public static int MaxDistance(ChunkCoords cc1, ChunkCoords cc2)
	{
		cc1 = ConvertToUpRight(cc1);
		cc2 = ConvertToUpRight(cc2);
		int x = cc1.X - cc2.X;
		x = x < 0 ? -x : x;
		int y = cc1.Y - cc2.Y;
		y = y < 0 ? -y : y;
		return Math.Max(x, y);
	}

	/// Converts the x and y components of a coordinate set so that they are easier to compare
	private static ChunkCoords ConvertToUpRight(ChunkCoords cc)
	{
		if (cc.Direction == Quadrant.UpperRight)
		{
			return cc;
		}

		//check if it is in either of the two left-side quadrants
		if (cc.Direction == Quadrant.UpperLeft || cc.Direction == Quadrant.LowerLeft)
		{
			cc.X = -cc.X - 1;
		}

		//check if it is in either of the two bottom quadrants
		if (cc.Direction == Quadrant.LowerRight || cc.Direction == Quadrant.LowerLeft)
		{
			cc.Y = -cc.Y - 1;
		}

		cc.Direction = Quadrant.UpperRight;
		return cc;
	}

	public override string ToString()
	{
		return string.Format("Direction: {0}, Coordinates({1}, {2})", Direction, X, Y);
	}

	public static bool operator ==(ChunkCoords c1, ChunkCoords c2)
	{
		return c1.Direction == c2.Direction && c1.X == c2.X && c1.Y == c2.Y;
	}

	public static bool operator !=(ChunkCoords c1, ChunkCoords c2)
	{
		return c1.Direction != c2.Direction || c1.X != c2.X || c1.Y != c2.Y;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}