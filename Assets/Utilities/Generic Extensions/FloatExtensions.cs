using UnityEngine;

namespace GenericExtensions
{
	public static class FloatExtensions
	{
		public static Vector2 DegreeAngleToVector2(this float source)
			=> (source * Mathf.Deg2Rad).RadiansAngleToVector2();

		public static Vector2 RadiansAngleToVector2(this float source)
			=> new Vector2(Mathf.Sin(source), Mathf.Cos(source));
	}
}
