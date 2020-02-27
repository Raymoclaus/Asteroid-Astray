using System.Collections.Generic;
using SaveSystem;
using UnityEngine;

public class SaveFileCardGenerator : MonoBehaviour
{
	[SerializeField] private SaveFileCardController placecardPrefab;

	private void Awake()
	{
		//remove existing children
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}
		//get list of save files
		List<SaveFile> saves = SaveReader.GetSaves();
		//read save files and create placecards for each one
		foreach (SaveFile save in saves)
		{
			SaveFileCardController sfcc = Instantiate(placecardPrefab, transform);
			sfcc.SetFileName(save.directoryName);
		}
	}
}
