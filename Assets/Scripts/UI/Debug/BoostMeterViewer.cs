using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(RawImage))]
public class BoostMeterViewer : MonoBehaviour
{
	[SerializeField]
	private RawImage bar;

	private void OnGUI()
	{
		if (Shuttle.singleton == null) return;

		float delta = Shuttle.singleton.GetBoostRemaining();
		Vector3 scl = Vector3.one;
		scl.x = delta;
		bar.transform.localScale = scl;
		bar.color = delta == 1f ? Color.yellow : Color.white;
	}
}