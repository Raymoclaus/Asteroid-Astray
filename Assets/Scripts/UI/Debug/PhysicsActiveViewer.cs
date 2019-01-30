using UnityEngine;
using UnityEngine.UI;
using System.Text;

[RequireComponent(typeof(Text))]
public class PhysicsActiveViewer : MonoBehaviour
{
	private Text txt;
	private int currentCount;
	private string display = "Physics Active: {0}";
	private StringBuilder sb;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		int count = Entity.GetActive();
		if (count != currentCount)
		{
			sb.Clear();
			sb.AppendFormat(display, count);
			txt.text = sb.ToString();
			currentCount = count;
		}
	}
}
