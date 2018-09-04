using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipEntryPanel : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ClosePanel();
		}
	}

	public void ClosePanel()
	{
		Pause.InstantPause(false);
		gameObject.SetActive(false);
	}
}