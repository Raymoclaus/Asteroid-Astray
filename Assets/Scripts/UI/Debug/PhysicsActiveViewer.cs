using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class PhysicsActiveViewer : MonoBehaviour
{
	private Text txt;
	private int currentCount;
	private const string display = "Physics Active: ";

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		int count = Entity.GetActive();
		if (count != currentCount)
		{
			txt.text = display + count;
			currentCount = count;
		}
	}
}
