using UnityEngine;

namespace InputHandlerSystem
{
	public class SceneContextInitialiser : MonoBehaviour
	{
		[SerializeField] private string defaultContext = "Menu";

		private void Awake()
		{
			InputManager.SetContext(defaultContext);
		}
	} 
}
