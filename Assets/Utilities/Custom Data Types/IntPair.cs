using System;
using UnityEngine;

namespace CustomDataTypes
{
	[Serializable]
	public struct IntPair
	{
		public int x, y;

		public IntPair(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public IntPair(Vector2 pos) : this((int)pos.x, (int)pos.y) { }

		public IntPair(Vector3 pos) : this((Vector2)pos) { }

		public static IntPair up = new IntPair(0, 1);

		public static IntPair down = new IntPair(0, -1);

		public static IntPair left = new IntPair(-1, 0);

		public static IntPair right = new IntPair(1, 0);

		public static IntPair one = new IntPair(1, 1);

		public static IntPair zero = new IntPair(0, 0);

		public static IntPair GetDirection(Direction dir)
		{
			switch (dir)
			{
				default: return zero;
				case Direction.Up: return up;
				case Direction.Right: return right;
				case Direction.Down: return down;
				case Direction.Left: return left;
			}
		}

		public override string ToString() => $"({x}, {y})";

		public static bool TryParse(string toParse, out IntPair result)
		{
			try
			{
				toParse = toParse.Replace(" ", string.Empty)
					.Replace("(", string.Empty)
					.Replace(")", string.Empty);
				string[] integerStrings = toParse.Split(',');
				int.TryParse(integerStrings[0], out result.x);
				int.TryParse(integerStrings[1], out result.y);
				return true;
			}
			catch (ArgumentException e)
			{
				Debug.LogError(e);
				result = zero;
				return false;
			}
			catch (IndexOutOfRangeException e)
			{
				Debug.LogError(e);
				result = zero;
				return false;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				result = zero;
				return false;
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IntPair))
			{
				return false;
			}

			var pair = (IntPair)obj;
			return x == pair.x &&
				   y == pair.y;
		}

		public override int GetHashCode()
		{
			var hashCode = 1502939027;
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(IntPair a, IntPair b) => a.x == b.x && a.y == b.y;

		public static bool operator !=(IntPair a, IntPair b) => a.x != b.x || a.y != b.y;

		public static IntPair operator +(IntPair a, IntPair b) => new IntPair(a.x + b.x, a.y + b.y);

		public static IntPair operator -(IntPair a, IntPair b) => new IntPair(a.x - b.x, a.y - b.y);

		public static IntPair operator *(IntPair a, IntPair b) => new IntPair(a.x * b.x, a.y * b.y);

		public static IntPair operator /(IntPair a, IntPair b) => new IntPair(a.x / b.x, a.y / b.y);

		public static IntPair operator *(IntPair a, int b) => new IntPair(a.x * b, a.y * b);

		public static IntPair operator /(IntPair a, int b) => new IntPair(a.x / b, a.y / b);

		public static bool operator ==(Vector2 a, IntPair b) => (int)a.x == b.x && (int)a.y == b.y;

		public static bool operator !=(Vector2 a, IntPair b) => (int)a.x != b.x || (int)a.y != b.y;

		public static Vector2 operator +(Vector2 a, IntPair b) => new Vector2(a.x + b.x, a.y + b.y);

		public static Vector2 operator -(Vector2 a, IntPair b) => new Vector2(a.x - b.x, a.y - b.y);

		public static Vector2 operator *(Vector2 a, IntPair b) => new Vector2(a.x * b.x, a.y * b.y);

		public static Vector2 operator /(Vector2 a, IntPair b) => new Vector2(a.x / b.x, a.y / b.y);

		public static Vector3 operator +(Vector3 a, IntPair b) => new Vector3(a.x + b.x, a.y + b.y);

		public static Vector3 operator -(Vector3 a, IntPair b) => new Vector3(a.x - b.x, a.y - b.y);

		public static Vector3 operator *(Vector3 a, IntPair b) => new Vector3(a.x * b.x, a.y * b.y);

		public static Vector3 operator /(Vector3 a, IntPair b) => new Vector3(a.x / b.x, a.y / b.y);

		public static implicit operator IntPair(Vector2 pos) => new IntPair(pos);

		public static implicit operator Vector2(IntPair pos) => new Vector2(pos.x, pos.y);

		public static implicit operator IntPair(Vector3 pos) => new IntPair(pos);

		public static implicit operator Vector3(IntPair pos) => new Vector2(pos.x, pos.y);
	}
}