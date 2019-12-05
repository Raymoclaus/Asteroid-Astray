using UnityEngine;

public class YPositionSpriteSorting : MonoBehaviour
{
	private const int PRECISION = 1000;
	[SerializeField] private SpriteRenderer sprRend;
	[SerializeField] private Transform pivot;
	private float yPosition = float.NaN;

	private void Update()
	{
		float currentY = pivot.position.y;
		if (yPosition == currentY) return;
		yPosition = currentY;
		sprRend.sortingOrder = (int)(-yPosition * PRECISION);
	}
}
