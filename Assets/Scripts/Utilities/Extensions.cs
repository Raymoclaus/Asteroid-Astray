using System;
using UnityEngine;

public static class Extensions
{
	public static T[] SubArray<T>(this T[] source, int start, int end)
	{
		if (start >= source.Length || end >= source.Length || start > end)
		{
			return null;
		}

		int length = end - start;
		T[] result = new T[length];

		for (int i = 0; i < length; i++)
		{
			result[i] = source[start + i];
		}

		return result;
	}

	public static int IndexOfObjectAfterIndex<T>(this T[] source, int startingIndex, T objectToFind)
	{
		if (startingIndex >= source.Length) return -1;
		startingIndex = Mathf.Max(0, startingIndex);

		for (int i = startingIndex; i < source.Length; i++)
		{
			if (source[i].Equals(objectToFind)) return i;
		}
		return -1;
	}

	public static Direction Opposite(this Direction source)
	{
		switch (source)
		{
			default: return Direction.Up;
			case Direction.Up: return Direction.Down;
			case Direction.Right: return Direction.Left;
			case Direction.Down: return Direction.Up;
			case Direction.Left: return Direction.Right;
		}
	}

	public static Direction Clockwise(this Direction source)
	{
		switch (source)
		{
			default: return Direction.Up;
			case Direction.Up: return Direction.Right;
			case Direction.Right: return Direction.Down;
			case Direction.Down: return Direction.Left;
			case Direction.Left: return Direction.Up;
		}
	}

	public static Direction AntiClockwise(this Direction source)
	{
		switch (source)
		{
			default: return Direction.Up;
			case Direction.Up: return Direction.Left;
			case Direction.Right: return Direction.Up;
			case Direction.Down: return Direction.Right;
			case Direction.Left: return Direction.Down;
		}
	}

	public static Direction ToPositiveDirection(this Direction source)
	{
		switch (source)
		{
			default: return source;
			case Direction.Down: return Direction.Up;
			case Direction.Left: return Direction.Right;
		}
	}

	public static bool IsVertical(this Direction source)
		=> source == Direction.Up || source == Direction.Down;

	public static bool IsHorizontal(this Direction source) => !source.IsVertical();

	public static Vector2 ToVector2(this Direction source)
	{
		switch (source)
		{
			default: return Vector2.up;
			case Direction.Up: return Vector2.up;
			case Direction.Right: return Vector2.right;
			case Direction.Down: return Vector2.down;
			case Direction.Left: return Vector2.left;
		}
	}

	public static int EnumEntryCount<T>(this T source) where T : Enum
		=> Enum.GetValues(source.GetType()).Length;

	public static T Random<T>(this T source) where T : Enum
	{
		Enum[] vals = (Enum[])Enum.GetValues(source.GetType());
		return (T)vals[UnityEngine.Random.Range(0, vals.Length)];
	}

	public static T ToEnum<T>(this int source) => (T)Enum.ToObject(typeof(T), source);

	public static void Print<T>(this T source) => Debug.Log(source);

	public static bool IsA<T>(this object source) => source is T;

	public static bool IsNotA<T>(this object source) => !source.IsA<T>();

	public static Vector2 DegreeAngleToVector2(this float source)
		=> (source * Mathf.Deg2Rad).RadiansAngleToVector2();

	public static Vector2 RadiansAngleToVector2(this float source)
		=> new Vector2(Mathf.Sin(source), Mathf.Cos(source));
}
