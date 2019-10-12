using System;
using UnityEngine;

namespace CustomDataTypes
{
	[Serializable]
	public struct ChunkCoords
	{
		public Quadrant Direction;
		public int x, y;

		public static ChunkCoords Invalid
		{
			get { return new ChunkCoords((Quadrant)(-1), -1, -1); }
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
			this.x = x;
			this.y = y;

			if (shouldValidate != null && (bool)shouldValidate)
			{
				this = Validate();
			}
		}

		public static ChunkCoords PosToCoords(Vector2 pos)
		{
			ChunkCoords cc;
			cc.Direction = GetDirection(pos);
			IntPair coord = ConvertToXy(pos);
			cc.x = coord.x;
			cc.y = coord.y;
			return cc;
		}

		private static IntPair ConvertToXy(Vector2 pos)
		{
			pos /= Constants.CHUNK_SIZE;
			return new IntPair(Math.Abs((int)pos.x), Math.Abs((int)pos.y));
		}

		public static Quadrant GetDirection(Vector2 pos)
		{
			pos /= Constants.CHUNK_SIZE;
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
			Vector2 min = new Vector2(chCoord.x, chCoord.y);
			Vector2 max = new Vector2(chCoord.x + 1, chCoord.y + 1);
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

			return new Vector2Pair(min * Constants.CHUNK_SIZE, max * Constants.CHUNK_SIZE);
		}

		public static Vector2 GetCenterCell(ChunkCoords c)
		{
			Vector2Pair bounds = GetCellArea(c);
			return new Vector2((bounds.a.x + bounds.b.x) / 2f, (bounds.a.y + bounds.b.y) / 2f);
		}

		public bool IsValid()
		{
			return this != Invalid
				   && (int)Direction >= 0
				   && (int)Direction < EntityNetwork.QUADRANT_COUNT
				   && x >= 0
				   && y >= 0;
		}

		public ChunkCoords Validate()
		{
			//if it is already valid then no changes required
			if (IsValid())
			{
				return this;
			}

			//fix direction to be within bounds
			Direction = (Quadrant)(Math.Abs((int)Direction) % EntityNetwork.QUADRANT_COUNT);
			//adjust direction if x is not valid
			if (x < 0)
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

				x = Math.Abs(x) - 1;
			}

			//adjust direction if y is not valid
			if (y < 0)
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

				y = Math.Abs(y) - 1;
			}

			return this;
		}

		/// Converts valid coordinates into unsigned X, Y coordinates.
		/// The result will usually be considered 'invalid' by IsValid()
		public ChunkCoords ConvertToSignedCoords()
		{
			//make sure the coordinates are valid
			if (!IsValid())
			{
				Validate();
			}
			//convert coordinates into signed X, Y values
			switch (Direction)
			{
				case Quadrant.UpperLeft:
					x = -x - 1;
					break;
				case Quadrant.LowerLeft:
					x = -x - 1;
					y = -y - 1;
					break;
				case Quadrant.LowerRight:
					y = -y - 1;
					break;
			}
			return this;
		}

		/// Returns the distance between two coordinates. (Diagonal distance is treated the same as axis distance)
		public static int MaxDistance(ChunkCoords cc1, ChunkCoords cc2)
		{
			cc1 = ConvertToUpRight(cc1);
			cc2 = ConvertToUpRight(cc2);
			int x = cc1.x - cc2.x;
			x = x < 0 ? -x : x;
			int y = cc1.y - cc2.y;
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
				cc.x = -cc.x - 1;
			}

			//check if it is in either of the two bottom quadrants
			if (cc.Direction == Quadrant.LowerRight || cc.Direction == Quadrant.LowerLeft)
			{
				cc.y = -cc.y - 1;
			}

			cc.Direction = Quadrant.UpperRight;
			return cc;
		}

		public override string ToString()
		{
			return string.Format("Direction: {0}, Coordinates({1}, {2})", Direction, x, y);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ChunkCoords))
			{
				return false;
			}

			var coords = (ChunkCoords)obj;
			return Direction == coords.Direction &&
				   x == coords.x &&
				   y == coords.y;
		}

		public override int GetHashCode()
		{
			var hashCode = -2054141635;
			hashCode = hashCode * -1521134295 + Direction.GetHashCode();
			hashCode = hashCode * -1521134295 + x.GetHashCode();
			hashCode = hashCode * -1521134295 + y.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(ChunkCoords c1, ChunkCoords c2)
			=> c1.Direction == c2.Direction && c1.x == c2.x && c1.y == c2.y;

		public static bool operator !=(ChunkCoords c1, ChunkCoords c2)
			=> c1.Direction != c2.Direction || c1.x != c2.x || c1.y != c2.y;

		public static ChunkCoords operator +(ChunkCoords c1, ChunkCoords c2)
			=> new ChunkCoords(c1.Direction, c1.x + c2.x, c1.y + c2.y, true);

		public static ChunkCoords operator -(ChunkCoords c1, ChunkCoords c2)
			=> new ChunkCoords(c1.Direction, c1.x - c2.x, c1.y - c2.y, true);

		public static ChunkCoords operator *(ChunkCoords c1, ChunkCoords c2)
			=> new ChunkCoords(c1.Direction, c1.x * c2.x, c1.y * c2.y, true);

		public static ChunkCoords operator /(ChunkCoords c1, ChunkCoords c2)
			=> new ChunkCoords(c1.Direction, c1.x / c2.x, c1.y / c2.y, true);
	}
}