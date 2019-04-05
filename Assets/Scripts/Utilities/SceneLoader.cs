using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
	public static SceneLoader instance;

	public delegate void SceneLoadEventHandler(string sceneName);
	public static event SceneLoadEventHandler OnSceneLoad;
	public static void ClearEvent()
	{
		System.Delegate[] delegates = OnSceneLoad?.GetInvocationList();
		for (int i = 0; i < delegates?.Length; i++)
		{
			OnSceneLoad -= (SceneLoadEventHandler)delegates[i];
		}
	}


	public List<string> sceneNames = new List<string>();

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		GetScenesFromBuild();
		//OnSceneLoad?.Invoke(SceneManager.GetActiveScene().name);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			LoadScene("MainMenuScene");
		}
	}

	public void LoadScene(string sceneName)
	{
		if (SceneExists(sceneName)) return;
		OnSceneLoad?.Invoke(sceneName);
		ClearEvents();
		SceneManager.LoadScene(sceneName);
	}

	private void ClearEvents()
	{
		InputHandler.ClearEvent();
		Shuttle.ClearEvent();
		GameEvents.ClearEvent();
		PromptUI.ClearEvent();
		LoadingController.ClearEvent();
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
