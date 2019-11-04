using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIMoveTrigger : MoveTrigger
{
	protected override void SetPosition(Vector3 pos)
	{
		if (useWorldSpace)
		{
			((RectTransform)tr).position = pos;
		}
		else
		{
			((RectTransform)tr).anchoredPosition3D = pos;
		}
	}
}
