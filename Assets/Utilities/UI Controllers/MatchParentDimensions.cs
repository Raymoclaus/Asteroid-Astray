using UnityEngine;

namespace UIControllers
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class MatchParentDimensions : MonoBehaviour
	{
		private RectTransform rt;
		private RectTransform Rt
			=> rt != null ? rt : (rt = GetComponent<RectTransform>());
		[SerializeField] private bool affectWidth, affectHeight;

		[ExecuteInEditMode]
		private void Awake() => MatchParent();

		[ExecuteInEditMode]
		private void OnTransformParentChanged() => MatchParent();

		[ExecuteInEditMode]
		private void OnRectTransformDimensionsChange() => MatchParent();

		[ExecuteInEditMode]
		private void OnValidate() => MatchParent();

		private RectTransform Parent => Rt.parent.GetComponent<RectTransform>();

		private void MatchParent()
		{
			if (!affectWidth && !affectHeight) return;

			//set pivot and anchor to center
			Vector2 center = Vector2.one * 0.5f;
			Rt.pivot = center;
			Vector2 anchorPos = Vector2.up;
			Rt.anchorMin = anchorPos;
			Rt.anchorMax = anchorPos;

			//get parent
			RectTransform parent = Parent;

			//get current size
			Vector2 size = Rt.sizeDelta;

			if (affectWidth)
			{
				//set width to parent's width
				size.x = parent.rect.width;
			}

			if (affectHeight)
			{
				//set height to parent's height;
				size.y = parent.rect.height;
			}

			//finalise changes
			Rt.sizeDelta = size;
		}
	}
}