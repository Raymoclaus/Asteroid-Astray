using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class ResourceViewer : MonoBehaviour
{
	private Text counter;
	private int currentCount;
	[SerializeField]
	private ShuttleTrackers shuttleTrackerSO;

	private void Awake()
	{
		counter = GetComponent<Text>();
	}

	private void Update()
	{
		if (!shuttleTrackerSO) return;

		int count = shuttleTrackerSO.storageCount;
		if (count != currentCount)
		{
			counter.text = count.ToString();
			currentCount = count;
		}
	}
}