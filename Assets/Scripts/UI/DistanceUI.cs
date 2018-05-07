using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Text))]
public class DistanceUI : MonoBehaviour
{
	public const int maxRange = 10000;
	private List<string> distStrings = new List<string>(maxRange + 1);
	private string unit = "m";
	private int dist = -1;
	private Text _textComponent;

	private void Start()
	{
		_textComponent = GetComponent<Text>();
		StartCoroutine(FillStrings());
	}

	private void Update()
	{
		if (Shuttle.singleton == null) return;

		int currentDist = (int)(Shuttle.singleton.transform.position.magnitude * 3f);
		if (dist != currentDist)
		{
			dist = currentDist;
			if (dist < maxRange && dist < distStrings.Count)
			{
				_textComponent.text = distStrings[dist];
			}
			else
			{
				_textComponent.text = distStrings[distStrings.Count - 1];
			}
		}
	}

	private IEnumerator FillStrings()
	{
		for (int i = 0; i < maxRange; i++)
		{
			distStrings.Add(i.ToString() + unit);
			if (i % 1000 == 999) yield return null;
		}
		distStrings.Add(maxRange.ToString() + "+" + unit);
	}
}