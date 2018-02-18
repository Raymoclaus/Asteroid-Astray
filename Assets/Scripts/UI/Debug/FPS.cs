using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPS : MonoBehaviour
{
	private Text txt;
	private float lastFrameTime;
	private float counter;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void OnGUI()
	{
		txt.text = string.Format("FPS: {0:0}", 1f / Time.deltaTime);
	}
}
