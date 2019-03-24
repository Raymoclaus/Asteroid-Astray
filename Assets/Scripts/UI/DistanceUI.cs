using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class DistanceUI : MonoBehaviour
{
	public const float UNITS_TO_METRES = 3f;
	public const int maxRange = 10000;
	private List<string> distStrings = new List<string>(maxRange + 1);
	private const string unit = "m";
	private int dist = -1;
	[SerializeField] private Text textComponent;
	[SerializeField] private ShuttleTrackers shuttleTrackerSO;

	private void Start()
	{
		textComponent = textComponent ?? GetComponent<Text>();
		StartCoroutine(FillStrings());
		shuttleTrackerSO.NavigationUpdated += UpdateHUD;
		UpdateHUD();
	}

	private void Update()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		if (!shuttleTrackerSO) return;
		int currentDist = (int)(shuttleTrackerSO.GetDistanceToWaypoint() * UNITS_TO_METRES);
		if (dist != currentDist)
		{
			dist = currentDist;
			if (dist < maxRange && dist < distStrings.Count)
			{
				textComponent.text = distStrings[dist];
			}
			else
			{
				textComponent.text = distStrings[distStrings.Count - 1];
			}
		}
	}

	private void UpdateHUD()
	{
		gameObject.SetActive(shuttleTrackerSO.navigationActive);
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