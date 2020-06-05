using UnityEngine;

namespace GenericExtensions
{
	public static class TransformExtensions
	{
		public static void DestroyAllChildren(this Transform source)
		{
			foreach (Transform child in source)
			{
				Object.Destroy(child.gameObject);
			}
		}
	}
}