using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class ResourceViewer : MonoBehaviour
{
	private Text counter;
	private int currentCount;
	private string display = "ResourceCounter";

	private void Awake()
	{
		counter = GetComponent<Text>();
	}

	private void Update()
	{
		int count = PlayerPrefs.GetInt(display);
		if (count != currentCount)
		{
			counter.text = count.ToString();
			currentCount = count;
		}
	}
}