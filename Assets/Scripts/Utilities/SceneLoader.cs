using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;
using InputHandler;

public class SceneLoader : MonoBehaviour
{
	private static SceneLoader instance;
	private static SceneLoader Instance => instance ??
		(instance = FindObjectOfType<SceneLoader>());

	public delegate void SceneLoadEventHandler(string sceneName);
	public event SceneLoadEventHandler OnSceneLoad;

	private List<string> sceneNames = new List<string>();

	private void Awake()
	{
		GetScenesFromBuild();
	}

	public static SceneAsync PrepareScene(string sceneName,
		System.Action<AsyncOperation> preparedAction = null)
	{
		AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
		ao.allowSceneActivation = false;
		ao.completed += preparedAction;
		return new SceneAsync(ao, sceneName);
	}

	public static void LoadPreparedSceneStatic(SceneAsync scene)
	{
		Instance.LoadPreparedScene(scene);
	}

	public static void LoadSceneStatic(string sceneName)
	{
		Instance.LoadScene(sceneName);
	}

	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void LoadPreparedScene(SceneAsync scene)
	{
		scene.ao.allowSceneActivation = true;
	}

	public void Quit()
	{
		if (Application.isEditor)
		{
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}
		else
		{
			Application.Quit();
		}
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

	public struct SceneAsync
	{
		public AsyncOperation ao;
		public string name;

		public SceneAsync(AsyncOperation ao, string name)
		{
			this.ao = ao;
			this.name = name;
		}
	}
}
