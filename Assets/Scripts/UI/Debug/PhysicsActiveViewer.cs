using UnityEngine;
using UnityEngine.UI;
using System.Text;

[RequireComponent(typeof(Text))]
public class PhysicsActiveViewer : MonoBehaviour
{
	private Text txt;
	private int currentCount;
	private string display = "Physics Active: {0}";

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		int count = Entity.GetActive();
		if (count != currentCount)
		{
			txt.text = string.Format(display, count);
			currentCount = count;
		}
	}
}
