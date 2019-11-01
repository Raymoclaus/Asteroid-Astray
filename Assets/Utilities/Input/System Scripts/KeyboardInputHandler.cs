using UnityEngine;

namespace InputHandlerSystem
{
	public class KeyboardInputHandler : CustomInputHandler
	{
		//used for checking if the mouse has moved
		private Vector2 _prevMousePos;

		public override float GetLookAngle(Vector2 refLocation)
		{
			if (!Camera.main) return 0f;

			Vector2 cursorPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
			return -Vector2.SignedAngle(Vector2.up, cursorPos - refLocation);
		}

		//checks all methods of input to determine if mouse/keyboard is in use, excludes non-bound inputs
		public override bool ProcessInputs(InputContext context)
		{
			//checks if the mouse has moved
			if (_prevMousePos != (Vector2)UnityEngine.Input.mousePosition)
			{
				//update mouse check variable
				_prevMousePos = UnityEngine.Input.mousePosition;
				return true;
			}

			return base.ProcessInputs(context);
		}

		public override InputMode GetInputMode() => InputMode.Keyboard;
	}
}
