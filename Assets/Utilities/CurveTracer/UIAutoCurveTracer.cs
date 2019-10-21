using UnityEngine;

namespace CurveTracerSystem
{
	[RequireComponent(typeof(RectTransform))]
	public class UIAutoCurveTracer : AutoCurveTracer
	{
		private RectTransform rect;
		private RectTransform Rect => rect != null ? rect
			: (rect = GetComponent<RectTransform>());

		protected override Vector3 SetPosition(Vector3 pos)
			=> Rect == null ? Vector3.zero : (Vector3)(Rect.anchoredPosition = pos);
	}
}
