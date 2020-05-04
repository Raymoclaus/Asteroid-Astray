using UnityEngine;

namespace InputHandlerSystem
{
	public class SceneContextInitialiser : MonoBehaviour
	{
		[SerializeField] private InputContext _defaultContext;

		private void Awake()
		{
			SetContext();
		}

		public void SetContext()
		{
			InputManager.SetCurrentContext(_defaultContext);
		}
	} 
}
