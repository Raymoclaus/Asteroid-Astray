using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class EntityCountViewer : MonoBehaviour
{
	private Text txt;
	private int currentCount;
	private string display = "Entity Count: {0}";

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		int count = EntityNetwork.GetEntityCount();
		if (count != currentCount)
		{
			txt.text = string.Format(display, count);
			currentCount = count;
		}
	}
}
