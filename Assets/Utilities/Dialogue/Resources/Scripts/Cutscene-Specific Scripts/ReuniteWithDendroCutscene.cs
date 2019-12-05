using System.Collections.Generic;
using StatisticsTracker;
using UnityEngine;

public class ReuniteWithDendroCutscene : MonoBehaviour
{
	[SerializeField] private IntStatTracker hubEnteredCounter;

	[SerializeField] private List<GameObject> cutsceneSpecificObjects;
	[SerializeField] private List<GameObject> objectsToHideDuringCutscene;

	private void Start()
	{
		if (hubEnteredCounter.value == 1)
		{
			//activate cutscene-specific objects
			ActivateGameObjects(cutsceneSpecificObjects, true);
			//deactivate unnecessary objects for cutscene
			ActivateGameObjects(objectsToHideDuringCutscene, false);
		}
		else
		{
			//activate cutscene-specific objects
			ActivateGameObjects(cutsceneSpecificObjects, false);
		}
	}

	private void ActivateGameObjects(List<GameObject> objs, bool activate)
	{
		for (int i = 0; i < objs.Count; i++)
		{
			GameObject go = objs[i];
			go.SetActive(activate);
		}
	}
}
