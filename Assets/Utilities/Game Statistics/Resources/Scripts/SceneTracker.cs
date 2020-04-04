using SceneControllers;

namespace StatisticsTracker
{
	public static class SceneTracker
	{
		private const string CURRENT_SCENE_STAT_NAME = "Current Scene",
			PREVIOUS_SCENE_STAT_NAME = "Previous Scene";

		public static void AttachToSceneLoader()
		{
			SceneLoader.OnSceneLoad += SceneLoading;
		}

		private static void SceneLoading(string sceneName)
		{
			StatTracker currentSceneStat = StatisticsIO.GetTracker(CURRENT_SCENE_STAT_NAME);
			currentSceneStat.Parse(sceneName);
			StatTracker previousSceneStat = StatisticsIO.GetTracker(PREVIOUS_SCENE_STAT_NAME);
			previousSceneStat.Parse(SceneLoader.CurrentSceneName);
		}
	} 
}
