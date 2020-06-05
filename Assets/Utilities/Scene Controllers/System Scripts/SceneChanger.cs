using UnityEngine;

namespace SceneControllers
{
	public class SceneChanger : MonoBehaviour
	{
		private SceneLoader.SceneAsync loadingScene;

		public void LoadScene(string sceneName) => SceneLoader.LoadScene(sceneName);

		public void PrepareScene(string sceneName)
			=> loadingScene = SceneLoader.PrepareScene(sceneName);

		public void LoadPreparedScene() => SceneLoader.LoadPreparedScene(loadingScene);

		public void ResetScene()
		{
			SceneLoader.ResetScene();
			Debug.Log("Resetting Scene");
		}

		public void Quit() => SceneLoader.Quit();
	}
}