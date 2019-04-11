using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
	public delegate void SceneLoadEventHandler(string sceneName);
	public static event SceneLoadEventHandler OnSceneLoad;
	private static void ClearEvent() => OnSceneLoad = null;

	private List<string> sceneNames = new List<string>();

	private void Awake()
	{
		GetScenesFromBuild();
	}

	public void LoadScene(string sceneName)
	{
		if (SceneExists(sceneName)) return;
		OnSceneLoad?.Invoke(sceneName);
		ClearEvents();
		SceneManager.LoadScene(sceneName);
	}

	public void Quit()
	{
		Application.Quit();
	}

	private void ClearEvents()
	{
		InputHandler.ClearEvent();
		Shuttle.ClearEvent();
		GameEvents.ClearEvent();
		PromptUI.ClearEvent();
		Pause.ClearEvent();
	}

	private bool SceneExists(string sceneName)
	{
		for (int i = 0; i < sceneNames.Count; i++)
		{
			if (sceneNames[i] == sceneName) return true;
		}
		return false;
	}

	private void GetScenesFromBuild()
	{
		int sceneCount = SceneManager.sceneCountInBuildSettings;
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			sceneNames.Add(SceneUtility.GetScenePathByBuildIndex(i));
		}
	}
}
