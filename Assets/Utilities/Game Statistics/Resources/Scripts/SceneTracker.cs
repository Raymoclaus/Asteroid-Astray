using SceneControllers;
using UnityEngine;

namespace StatisticsTracker
{
	public static class SceneTracker
	{
		private const string CURRENT_SCENE_STAT_NAME = "Current Scene",
			PREVIOUS_SCENE_STAT_NAME = "Previous Scene";

		[RuntimeInitializeOnLoadMethod]
		public static void AttachToSceneLoader()
		{
			Debug.Log("Scene Tracker attached to Scene Loader");
			SceneLoader.OnSceneLoad += SceneLoading;
		}

		private static void SceneLoading(string sceneName)
		{
			StatTracker currentSceneStat = StatisticsIO.GetTracker(CURRENT_SCENE_STAT_NAME);
			currentSceneStat.TryParse(sceneName);
			StatTracker previousSceneStat = StatisticsIO.GetTracker(PREVIOUS_SCENE_STAT_NAME);
			previousSceneStat.TryParse(SceneLoader.CurrentSceneName);
		}
	} 
}
