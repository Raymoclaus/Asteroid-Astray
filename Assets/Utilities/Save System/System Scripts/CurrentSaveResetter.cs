using SaveSystem;
using UnityEngine;

public class CurrentSaveResetter : MonoBehaviour
{
	private void Start()
	{
		ResetCurrentSave();
	}

	public void ResetCurrentSave()
	{
		SaveLoad.CurrentSave = null;
	}
}
