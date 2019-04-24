using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class BoostMeterViewer : MonoBehaviour
{
	[SerializeField] private Image bar;
	private float previousDelta = 1f;
	private Shuttle mainChar;
	private Shuttle MainChar { get { return mainChar ?? (mainChar = FindObjectOfType<Shuttle>()); } }

	private void Awake() => bar.fillMethod = Image.FillMethod.Horizontal;

	private void Update()
	{
		float delta = MainChar.GetBoostRemaining();
		if (Mathf.Approximately(previousDelta, delta)) return;
		previousDelta = delta;
		bar.fillAmount = delta;
		bar.color = delta == 1f ? Color.yellow : Color.white;
	}
}