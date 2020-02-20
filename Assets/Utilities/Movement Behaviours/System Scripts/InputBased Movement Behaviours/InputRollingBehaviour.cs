using InputHandlerSystem;
using UnityEngine;

namespace MovementBehaviours
{
	public class InputRollingBehaviour : MovementBehaviour
	{
		[SerializeField] private GameAction rollAction;

		private void Update()
		{
			if (ShouldRoll)
			{
				TriggerRoll(MovementDirection);
			}
		}

		private bool ShouldRoll
			=> InputManager.GetInputDown(rollAction);
	}

}