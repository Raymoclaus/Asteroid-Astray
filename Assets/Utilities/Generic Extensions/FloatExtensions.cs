using UnityEngine;

namespace GenericExtensions
{
	public static class FloatExtensions
	{
		public static Vector2 DegreeAngleToVector2(this float source)
			=> (source * Mathf.Deg2Rad).RadiansAngleToVector2();

		public static Vector2 RadiansAngleToVector2(this float source)
			=> new Vector2(Mathf.Sin(source), Mathf.Cos(source));

		public static void EnsureMinMax(out float min, out float max, params float[] values)
		{
			min = Mathf.Min(values);
			max = Mathf.Max(values);
		}

		public static float Difference(this float source, float other)
		{
			EnsureMinMax(out float min, out float max, source, other);
			return max - min;
		}

		public static float WrapBetween(this float source, float minimum, float maximum)
		{
			EnsureMinMax(out float min, out float max, minimum, maximum);
			float diff = max - min;
			float mod = (source - minimum) % diff;
			float wrapNegative = mod + (mod < 0f ? diff : 0f);
			float returnToRange = wrapNegative + min;
			return returnToRange;
		}

		public static float AngleDifference(this float source, float other, out bool clockwise)
		{
			float wrappedSource = source.WrapBetween(0f, 360f);
			float wrappedOther = other.WrapBetween(0f, 360f);
			EnsureMinMax(out float min, out float max, wrappedSource, wrappedOther);
			float difference = wrappedSource.Difference(wrappedOther);
			clockwise = (difference <= 180f) == (min == wrappedSource);
			if (difference > 180f)
			{
				difference = 360f - difference;
			}

			return difference;
		}
	}
}
