using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(RawImage))]
public class BoostMeterViewer : MonoBehaviour
{
	[SerializeField]
	private RawImage bar;
	[SerializeField]
	private ShuttleTrackers shuttleTrackerSO;
	private float previousDelta = 1f;

	private void Update()
	{
		if (!shuttleTrackerSO) return;

		float delta = shuttleTrackerSO.boostRemaining;
		if (Mathf.Approximately(previousDelta, delta)) return;
		previousDelta = delta;
		Vector3 scl = Vector3.one;
		scl.x = delta;
		bar.transform.localScale = scl;
		bar.color = delta == 1f ? Color.yellow : Color.white;
	}
}