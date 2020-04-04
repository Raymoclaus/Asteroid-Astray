using UnityEngine;

namespace GenericExtensions
{
	public static class ObjectExtensions
	{
		public static void Print(this object source) => Debug.Log(source);

		public static bool IsA<T>(this object source) => source is T;

		public static bool IsNotA<T>(this object source) => !source.IsA<T>();

		public static int FieldCount(this object source) => source.GetType().GetFields().Length;
	}

}