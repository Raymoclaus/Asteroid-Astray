using UnityEngine;

namespace InputHandlerSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/Game Action")]
	public class GameAction : ScriptableObject
	{
		[SerializeField] private string actionName;
		public string ActionName => actionName;
		[SerializeField] private InputContext context;
		public InputContext IntendedContext => context;
	} 
}
