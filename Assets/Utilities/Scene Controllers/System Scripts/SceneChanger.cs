using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneControllers
{
	public class SceneChanger : MonoBehaviour
	{
		public void LoadScene(string sceneName) => SceneLoader.LoadScene(sceneName);

		public void Quit() => SceneLoader.Quit();
	}
}