using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIAutoCurveTracer : AutoCurveTracer
{
	private RectTransform rect;
	private RectTransform Rect { get { return rect ?? (rect = GetComponent<RectTransform>()); } }

	protected override Vector3 SetPosition(Vector3 pos)
		=> Rect == null ? Vector3.zero : (Vector3)(Rect.anchoredPosition = pos);
}
