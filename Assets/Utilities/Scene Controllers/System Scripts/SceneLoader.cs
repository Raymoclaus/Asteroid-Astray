using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

namespace SceneControllers
{
	public static class SceneLoader
	{
		public static event Action<string> OnSceneLoad;

		private static List<string> sceneNames;
		private static List<string> SceneNames => sceneNames != null ? sceneNames
			: (sceneNames = GetScenesFromBuild());

		public static SceneAsync PrepareScene(string sceneName,
			Action<AsyncOperation> preparedAction = null)
		{
			AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
			ao.allowSceneActivation = false;
			ao.completed += preparedAction;
			return new SceneAsync(ao, sceneName);
		}

		[SteamPunkConsoleCommand(command = "scene", info = "Changes scene to one with given name. Use scenelist to get a list of scene names.")]
		public static void LoadScene(string sceneName)
		{
			OnSceneLoad?.Invoke(sceneName);
			SceneManager.LoadScene(sceneName);
		}

		[SteamPunkConsoleCommand(command = "scenelist", info = "Prints a list of scene names, usable with scene command.")]
		public static void PrintSceneList()
		{
			SteamPunkConsole.WriteLine("Scene names\n==================");
			for (int i = 0; i < SceneNames.Count; i++)
			{
				SteamPunkConsole.WriteLine(SceneNames[i]);
			}
		}

		public static void LoadPreparedScene(SceneAsync scene)
		{
			OnSceneLoad?.Invoke(scene.name);
			scene.ao.allowSceneActivation = true;
		}

		public static void Quit()
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
		

		private static bool SceneExists(string sceneName)
		{
			for (int i = 0; i < SceneNames.Count; i++)
			{
				if (SceneNames[i] == sceneName) return true;
			}
			return false;
		}

		private static List<string> GetScenesFromBuild()
		{
			List<string> names = new List<string>();
			int sceneCount = SceneManager.sceneCountInBuildSettings;
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				names.Add(SceneUtility.GetScenePathByBuildIndex(i));
			}
			return names;
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

			public void LoadSceneWhenReady()
			{
				ao.allowSceneActivation = true;
			}
		}
	}
}