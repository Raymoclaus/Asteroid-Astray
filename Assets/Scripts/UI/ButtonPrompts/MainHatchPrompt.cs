using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainHatchPrompt : MonoBehaviour, IPromptRespone
{
	public GameObject mainHutchUI;
	public string interactString;
	private KeyCode interactKey = KeyCode.E;

	public bool CheckResponse()
	{
		return Input.GetKey(interactKey);
	}

	public void Enter()
	{

	}

	public void Execute()
	{
		mainHutchUI.SetActive(true);
	}

	public void Exit()
	{
		mainHutchUI.SetActive(false);
	}

	public string InteractString()
	{
		return string.Format(interactString, interactKey.ToString());
	}
}