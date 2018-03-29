using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class ShipResourceViewer : MonoBehaviour
{
	private Text counter;
	private int currentCount;

	private void Awake()
	{
		counter = GetComponent<Text>();
	}

	private void Update()
	{
		if (ShipInventory.singleton.inventory.Count > 0)
		{
			int count = ShipInventory.singleton.Count(Item.Type.Stone);
			if (count != currentCount)
			{
				counter.text = count.ToString();
				currentCount = count;
			}
		}
	}
}