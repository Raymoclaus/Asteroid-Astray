using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UIControllers
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public abstract class CellSizing : MonoBehaviour
	{
		private RectTransform rt;
		private RectTransform Rt
			=> rt != null ? rt : (rt = GetComponent<RectTransform>());
		[SerializeField] private List<float> positions;
		private int childCount = 0;

		private void Awake() => UpdateChildren();

		private void OnValidate() => UpdateChildren();

		private void OnTransformChildrenChanged() => UpdateChildren();

		private void OnRectTransformDimensionsChange() => UpdateChildren();

		private void UpdateChildren()
		{
			UpdatePositionCount();

			if (childCount == 0) return;

			Rect r = Rt.rect;

			for (int i = 0; i < childCount; i++)
			{
				float min = i == 0 ? 0f : positions[i - 1];
				float max = i == positions.Count ? 1f : positions[i];
				//get child
				RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();

				//set pivot to center
				child.pivot = Vector2.one * 0.5f;

				//set position to calculated position
				Vector2 pos = child.anchoredPosition;
				pos = CalculatePosition(pos, min, max, r);
				child.anchoredPosition = pos;

				//set size dimensions
				Vector2 size = child.sizeDelta;
				size = CalculateSize(size, min, max, r);
				child.sizeDelta = size;
			}
		}

		protected abstract Vector2 CalculatePosition(
			Vector2 originalPos, float min, float max, Rect r);

		protected abstract Vector2 CalculateSize(
			Vector2 originalSize, float min, float max, Rect r);

		private void UpdatePositionCount()
		{
			//positions should contains (childCount - 1) elements
			childCount = 0;
			for (int i = 0; i < transform.childCount; i++)
			{
				LayoutElement le = transform.GetChild(i).GetComponent<LayoutElement>();
				if (le != null && le.ignoreLayout) continue;
				childCount++;
			}
			int targetElements = Mathf.Max(0, childCount - 1);

			positions = positions != null ? positions : new List<float>();

			while (positions.Count > targetElements)
			{
				positions.RemoveAt(positions.Count - 1);
			}

			int countDifference = Mathf.Abs(positions.Count - targetElements);
			if (countDifference != 0)
			{
				float last = positions.LastOrDefault();
				float increments = (1f - last) / (countDifference + 1);
				for (int i = 0; i < countDifference; i++)
				{
					positions.Add(last + increments * (i + 1));
				}
			}

			for (int i = 0; i < positions.Count; i++)
			{
				float min = i == 0 ? 0f : positions[i - 1];
				float max = i == positions.Count - 1 ? 1f : positions[i + 1];
				positions[i] = Mathf.Clamp(positions[i], min, max);
			}
		}
	}

}