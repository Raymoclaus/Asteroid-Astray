using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ShakeEffectUi : ShakeEffect
{
	private RectTransform rect;
	private RectTransform Rect { get { return rect ?? (rect = GetComponent<RectTransform>()); } }

	protected override void SetPosition(Vector3 pos) => Rect.anchoredPosition = pos;
}
