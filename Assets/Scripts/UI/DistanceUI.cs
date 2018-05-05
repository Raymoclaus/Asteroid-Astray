using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DistanceUI : MonoBehaviour
{
	public const int maxRange = 1000;
	private string[] distStrings;
	private string unit = "m";
	private int dist = -1;

	private Text _textComponent;

	private void Start()
	{
		_textComponent = GetComponent<Text>();

		distStrings = new string[maxRange + 1];
		for (int i = 0; i < maxRange; i++)
		{
			distStrings[i] = i.ToString() + unit;
		}
		distStrings[maxRange] = maxRange.ToString() + "+" + unit;
	}

	private void Update()
	{
		int currentDist = (int)(Shuttle.singleton.transform.position.magnitude * 3f);
		if (dist != currentDist)
		{
			dist = currentDist;
			if (dist < maxRange)
			{
				_textComponent.text = distStrings[dist];
			}
			else
			{
				_textComponent.text = distStrings[maxRange];
			}
		}
	}
}