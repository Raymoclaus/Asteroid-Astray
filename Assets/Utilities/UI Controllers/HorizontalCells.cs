using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIControllers
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class HorizontalCells : CellSizing
	{
		[SerializeField] private bool stretchWidth, centerY;

		protected override Vector2 CalculatePosition(
			Vector2 originalPos, float min, float max, Rect r)
		{
			originalPos.x = Mathf.Lerp(r.xMin, r.xMax, min);
			if (!centerY) return originalPos;
			originalPos.y = r.center.y;
			return originalPos;
		}

		protected override Vector2 CalculateSize(
			Vector2 originalSize, float min, float max, Rect r)
		{
			if (!stretchWidth) return originalSize;
			float difference = max - min;
			originalSize.x = difference * r.width;
			return originalSize;
		}
	}
}