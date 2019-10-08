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
	private float dist = -1;
	[SerializeField] private Text textComponent;
	[SerializeField] private Image img;
	[SerializeField] private Shuttle mainChar;
	private Shuttle MainChar
		=> mainChar ?? (mainChar = FindObjectOfType<Shuttle>());

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
		float currentDist = DistanceToWaypoint;
		if ((int)dist != (int)currentDist)
		{
			dist = currentDist;
			int zone = Difficulty.DistanceBasedDifficulty(
				ChunkCoords.GetCenterCell(CharacterCoordinates).magnitude);
			if (dist < maxRange && dist < distStrings.Count)
			{
				textComponent.text = distStrings[(int)(dist * UNITS_TO_METRES)] + zone;
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

	private ChunkCoords CharacterCoordinates
		=> MainChar?.GetCoords() ?? ChunkCoords.Zero;

	private Vector2 CurrentPosition
		=> MainChar?.transform.position ?? Vector3.zero;

	private float DistanceToWaypoint
		=> Vector2.Distance(WaypointPosition, CurrentPosition);

	private Vector2 WaypointPosition
		=> MainChar?.waypoint.GetPosition() ?? Vector2.zero;
}