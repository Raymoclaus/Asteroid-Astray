using UnityEngine;

namespace UIControllers
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class VerticalCells : CellSizing
	{
		[SerializeField] private bool stretchHeight, centerX;

		protected override Vector2 CalculatePosition(
			Vector2 originalPos, float min, float max, Rect r)
		{
			originalPos.y = Mathf.Lerp(r.yMin, r.yMax, 1f - min);
			if (!centerX) return originalPos;
			originalPos.x = r.center.x;
			return originalPos;
		}

		protected override Vector2 CalculateSize(
			Vector2 originalSize, float min, float max, Rect r)
		{
			if (!stretchHeight) return originalSize;
			float difference = max - min;
			originalSize.y = difference * r.height;
			return originalSize;
		}
	}
}