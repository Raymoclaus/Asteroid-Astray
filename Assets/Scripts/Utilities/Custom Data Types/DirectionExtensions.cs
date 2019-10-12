using UnityEngine;

namespace CustomDataTypes
{
	public static class DirectionExtensions
	{
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
	}

}