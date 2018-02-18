using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TimeSpeedViewer : MonoBehaviour
{
	private Text txt;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		txt.text = string.Format("Time Speed: {0:0.0}", Cnsts.TIME_SPEED);
	}
}
