using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MousePositionViewer : MonoBehaviour
{
	private Text txt;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		txt.text = string.Format("Mouse Position X/Y: ({0}, {1})", Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
	}
}
