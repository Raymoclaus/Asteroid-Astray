using UnityEngine;

namespace Utilities.Input
{
	public class InputHandler : MonoBehaviour
	{
		//reference to singleton entity if it exists
		private static InputHandler _singletonRef;
		
		//possible input modes
		public enum InputMode
		{
			Keyboard, Ps4
		}

		//current input mode
		private InputMode _mode;
		//used to query the control scheme of a keyboard
		private KeyboardInputHandler _keyLayout;
		//used to query the control scheme of a PS4 controller
		private Ps4InputHandler _ps4Layout;

		private void Awake()
		{
			//set up singleton entity
			if (_singletonRef == null)
			{
				_singletonRef = this;
				DontDestroyOnLoad(gameObject);
			}
			else if (_singletonRef != this)
			{
				Destroy(gameObject);
			}
			
			_keyLayout = new KeyboardInputHandler();
			_ps4Layout = new Ps4InputHandler();
		}

		private void CheckForModeUpdate()
		{
			InputMode prevMode = _mode;

			//check if keyboard/mouse input detected
			if (_keyLayout.ProcessInputs())
			{
				_mode = InputMode.Keyboard;
			}
			//check if ps4 controller input detected
			else if (_ps4Layout.ProcessInputs())
			{
				_mode = InputMode.Ps4;
			}

			if (prevMode != _mode)
			{
				Debug.Log("Input mode set to: " + _mode);
			}
		}

		//returns the input status of a given command. 0f usually means no input.
		public static float GetInput(string key)
		{
			InputHandler ih = _singletonRef;
			
			//make sure we're using the right mode
			ih.CheckForModeUpdate();
			
			switch (ih._mode)
			{
				case InputMode.Keyboard:
					switch (key)
					{
						case "MoveHorizontal":
							return ih._keyLayout.GetInput("MoveRight") - ih._keyLayout.GetInput("MoveLeft");
						case "MoveVertical":
							return ih._keyLayout.GetInput("MoveUp") - ih._keyLayout.GetInput("MoveDown");
						default:
							return ih._keyLayout.GetInput(key);
					}
				case InputMode.Ps4:
					return ih._ps4Layout.GetInput(key);
				default:
					Debug.Log("Input Mode is not set.");
					return 0f;
			}
		}

		//returns the angle in degrees that the shuttle should turn to
		public static float GetLookDirection(Vector2? refLocation = null)
		{
			InputHandler ih = _singletonRef;
			
			//make sure we're using the right mode
			ih.CheckForModeUpdate();

			switch (ih._mode)
			{
				case InputMode.Keyboard:
					if (refLocation != null) return KeyboardInputHandler.GetLookDirection((Vector2) refLocation);
					Debug.Log("Reference location required in keyboard/mouse mode.");
					return 0f;
				case InputMode.Ps4:
					return Ps4InputHandler.GetLookDirection();
				default:
					Debug.Log("Input Mode is not set.");
					return 0f;
			}
		}

		//returns the current input mode
		public static InputMode GetMode()
		{
			return _singletonRef._mode;
		}
	}
}
