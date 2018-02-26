using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class ResourceViewer : MonoBehaviour
{
	private Text counter;

	private void Awake()
	{
		counter = GetComponent<Text>();
	}

	private void OnGUI()
	{
		counter.text = PlayerPrefs.GetInt("ResourceCounter").ToString();
	}
}