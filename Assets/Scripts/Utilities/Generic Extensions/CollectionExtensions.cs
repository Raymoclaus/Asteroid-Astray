using System;
using System.Collections;

namespace GenericExtensions
{
	public static class CollectionExtensions
	{
		public static bool IsNullOrEmpty(this IList source)
			=> source == null || source.Count == 0;

		public static T[] SubArray<T>(this T[] source, int start, int end)
		{
			if (start >= source.Length
				|| end >= source.Length
				|| start > end) return null;

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
			startingIndex = Math.Max(0, startingIndex);

			for (int i = startingIndex; i < source.Length; i++)
			{
				if (source[i].Equals(objectToFind)) return i;
			}
			return -1;
		}

		public static bool IsValidIndex<T>(this T source, int index)
			where T : ICollection
			=> source != null
			&& source.Count > 0
			&& index >= 0
			&& index < source.Count;
	}
}
