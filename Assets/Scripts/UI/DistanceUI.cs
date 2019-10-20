using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using CustomDataTypes;
using QuestSystem;

public class DistanceUI : MonoBehaviour
{
	private const string unit = "Zone: ";
	[SerializeField] private Text textComponent;
	[SerializeField] private Quester mainQuester;

	private void Start()
	{
		textComponent = textComponent ?? GetComponent<Text>();
	}

	private void Update()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		if (!textComponent.enabled) return;

		int zone = Difficulty.DistanceBasedDifficulty(
			ChunkCoords.GetCenterCell(CharacterCoordinates, EntityNetwork.CHUNK_SIZE).magnitude);
		textComponent.text = unit + zone;
	}

	public void Activate(bool active)
	{
		textComponent.enabled = active;
	}

	private ChunkCoords CharacterCoordinates
		=> new ChunkCoords(CurrentPosition, EntityNetwork.CHUNK_SIZE);

	private Vector2 CurrentPosition
		=> mainQuester.transform.position;
}