using UnityEngine;
using UnityEngine.UI;

namespace UIControllers
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
	public class AutoGridCellResizer : MonoBehaviour
	{
		private GridLayoutGroup glg;
		private GridLayoutGroup Glg
			=> glg != null ? glg : (glg = GetComponent<GridLayoutGroup>());
		private RectTransform rt;
		private RectTransform Rt
			=> rt != null ? rt : (rt = GetComponent<RectTransform>());
		[SerializeField] private Vector2 cellSizeLimit = Vector2.one;
		private Vector2 Spacing => Glg.spacing;

		[ExecuteInEditMode]
		private void OnTransformChildrenChanged()
		{
			UpdateCellSize();
		}

		public void UpdateCellSize()
		{
			int childCount = transform.childCount;
			if (childCount == 0)
			{
				Glg.cellSize = cellSizeLimit;
			}
			else
			{
				int basePowerOfTwo = CalculateBasePowerOfTwo(childCount);
				Vector2 spacingAmount = Spacing * (basePowerOfTwo - 1);
				Vector2 cellSize;
				cellSize = (cellSizeLimit - spacingAmount) / basePowerOfTwo;
				Glg.cellSize = cellSize;
			}
		}

		private int CalculateBasePowerOfTwo(int n)
		{
			int i = 0;
			while (i * i < n)
			{
				i++;
			}
			return i;
		}
	}

}