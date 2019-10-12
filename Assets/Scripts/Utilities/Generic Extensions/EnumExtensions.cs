using System;

namespace GenericExtensions
{
	public static class EnumExtensions
	{
		public static int EnumEntryCount<T>(this T source) where T : Enum
			=> Enum.GetValues(source.GetType()).Length;

		public static T Random<T>(this T source) where T : Enum
		{
			Enum[] vals = (Enum[])Enum.GetValues(source.GetType());
			return (T)vals[UnityEngine.Random.Range(0, vals.Length)];
		}

		public static T ToEnum<T>(this int source)
			=> (T)Enum.ToObject(typeof(T), source);
	}
}
