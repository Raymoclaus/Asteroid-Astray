using System;

namespace GenericExtensions
{
	public static class EnumExtensions
	{
		/// <summary>
		/// The number of unique values (not names) in an enum. Caution: This causes allocation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static int EnumEntryCount<T>(this T source) where T : Enum
			=> Enum.GetValues(source.GetType()).Length;

		/// <summary>
		/// Gets a random value from an enum. Caution: This causes allocation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static T Random<T>(this T source) where T : Enum
		{
			Enum[] vals = (Enum[])Enum.GetValues(source.GetType());
			return (T)vals[UnityEngine.Random.Range(0, vals.Length)];
		}

		public static T ToEnum<T>(this int source)
			=> (T)Enum.ToObject(typeof(T), source);
	}
}
