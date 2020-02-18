using UnityEngine;

namespace InputHandlerSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/Input Action")]
	public class InputAction : ScriptableObject
	{
		[SerializeField] private string actionName;
		public string ActionName => actionName;
		[SerializeField] private InputContext context;
		public InputContext IntendedContext => context;

		public void ChangeName(string newName)
		{
			actionName = newName;
		}
	} 
}
