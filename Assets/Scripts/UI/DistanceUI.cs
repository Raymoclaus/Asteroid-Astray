using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class DistanceUI : MonoBehaviour
{
	public const float UNITS_TO_METRES = 3f;
	public const int maxRange = 10000;
	private List<string> distStrings = new List<string>(maxRange + 1);
	private const string unit = "m | zone: ";
	private int dist = -1;
	[SerializeField] private Text textComponent;
	[SerializeField] private Image img;
	[SerializeField] private Shuttle mainChar;
	private Shuttle MainChar { get { return mainChar ?? (mainChar = FindObjectOfType<Shuttle>()); } }

	private void Start()
	{
		textComponent = textComponent ?? GetComponent<Text>();
		StartCoroutine(FillStrings());
		MainChar.OnNavigationUpdated += Activate;
	}

	private void Update()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		if (!textComponent.enabled) return;
		int currentDist = (int)(DistanceToWaypoint() * UNITS_TO_METRES);
		if (dist != currentDist)
		{
			dist = currentDist;
			int zone = Difficulty.DistanceBasedDifficulty(dist);
			if (dist < maxRange && dist < distStrings.Count)
			{
				textComponent.text = distStrings[dist] + zone;
			}
			else
			{
				textComponent.text = distStrings[distStrings.Count - 1] + zone;
			}
		}
	}

	public void Activate(bool active)
	{
		textComponent.enabled = active;
		img.enabled = active;
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

	private Vector2 GetCurrentPosition() => MainChar?.transform.position ?? Vector2.zero;

	private float DistanceToWaypoint() => Vector2.Distance(GetWaypointPosition(), GetCurrentPosition());

	private Vector2 GetWaypointPosition() => MainChar?.waypoint.GetPosition() ?? Vector2.zero;
}