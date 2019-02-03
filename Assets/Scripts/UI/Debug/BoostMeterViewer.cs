using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class BoostMeterViewer : MonoBehaviour
{
	[SerializeField]
	private Image bar;
	[SerializeField]
	private ShuttleTrackers shuttleTrackerSO;
	private float previousDelta = 1f;

	private void Awake()
	{
		bar.fillMethod = Image.FillMethod.Horizontal;
	}

	private void Update()
	{
		if (!shuttleTrackerSO) return;

		float delta = shuttleTrackerSO.boostRemaining;
		if (Mathf.Approximately(previousDelta, delta)) return;
		previousDelta = delta;
		bar.fillAmount = delta;
		bar.color = delta == 1f ? Color.yellow : Color.white;
	}
}