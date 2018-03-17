using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class ResourceViewer : MonoBehaviour
{
	private Text counter;
	private int currentCount;

	private void Awake()
	{
		counter = GetComponent<Text>();
	}

	private void Update()
	{
		if (Shuttle.singleton.inventory.inventory.Count > 0)
		{
			int count = Shuttle.singleton.inventory.Count(Item.Type.Stone);
			if (count != currentCount)
			{
				counter.text = count.ToString();
				currentCount = count;
			}
		}
	}
}