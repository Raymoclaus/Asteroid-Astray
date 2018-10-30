using UnityEngine;

public static class InputHandler
{
	//possible input modes
	public enum InputMode
	{
		Keyboard, Ps4
	}

	//current input mode
	private static InputMode _mode;
	//used to query the control scheme of a keyboard
	private static KeyboardInputHandler keyLayout = new KeyboardInputHandler();
	//used to query the control scheme of a PS4 controller
	private static Ps4InputHandler ps4Layout = new Ps4InputHandler();

	private static void CheckForModeUpdate()
	{
		InputMode prevMode = _mode;

		//check if keyboard/mouse input detected
		if (keyLayout.ProcessInputs())
		{
			_mode = InputMode.Keyboard;
		}
		//check if ps4 controller input detected
		else if (ps4Layout.ProcessInputs())
		{
			_mode = InputMode.Ps4;
		}

		if (prevMode != _mode)
		{
			Debug.LogWarning("Input mode set to: " + _mode);
		}
	}

	//returns the input status of a given command. 0f usually means no input.
	public static float GetInput(string key)
	{
		int index = GetKeyIndex(key);
		if (index == -1)
		{
			Debug.LogWarning(string.Format("Action: {0}, does not exist.", key));
			return 0f;
		}

		//make sure we're using the right mode
		CheckForModeUpdate();

		switch (_mode)
		{
			case InputMode.Keyboard:
				return keyLayout.GetInput(key);
			case InputMode.Ps4:
				return ps4Layout.GetInput(key);
			default:
				Debug.LogError("Input Mode is not set.");
				return 0f;
		}
	}

	//returns the input status of a given command. 0f usually means no input.
	public static float GetInputDown(string key)
	{
		int index = GetKeyIndex(key);
		if (index == -1)
		{
			Debug.LogWarning(string.Format("Action: {0}, does not exist.", key));
			return 0f;
		}

		//make sure we're using the right mode
		CheckForModeUpdate();

		switch (_mode)
		{
			case InputMode.Keyboard:
				return keyLayout.GetInputDown(key);
			case InputMode.Ps4:
				return ps4Layout.GetInputDown(key);
			default:
				Debug.LogError("Input Mode is not set.");
				return 0f;
		}
	}

	//returns the input status of a given command. 0f usually means no input.
	public static float GetInputUp(string key)
	{
		int index = GetKeyIndex(key);
		if (index == -1)
		{
			Debug.LogWarning(string.Format("Action: {0}, does not exist.", key));
			return 0f;
		}

		//make sure we're using the right mode
		CheckForModeUpdate();

		switch (_mode)
		{
			case InputMode.Keyboard:
				return keyLayout.GetInputUp(key);
			case InputMode.Ps4:
				return ps4Layout.GetInputUp(key);
			default:
				Debug.LogError("Input Mode is not set.");
				return 0f;
		}
	}

	public static int GetKeyIndex(string key)
	{
		switch (key)
		{
			case "Go": return 0;
			case "Stop": return 1;
			case "Shoot": return 2;
			case "Boost": return 3;
			case "Pause": return 4;
			default: return -1;
		}
	}

	//returns the angle in degrees that the shuttle should turn to
	public static float GetLookDirection(Vector2? refLocation = null)
	{
		//make sure we're using the right mode
		CheckForModeUpdate();

		switch (_mode)
		{
			case InputMode.Keyboard:
				if (refLocation != null) return KeyboardInputHandler.GetLookDirection((Vector2)refLocation);
				Debug.LogError("Reference location required in keyboard/mouse mode.");
				return 0f;
			case InputMode.Ps4:
				return Ps4InputHandler.GetLookDirection();
			default:
				Debug.LogError("Input Mode is not set.");
				return 0f;
		}
	}

	//returns the current input mode
	public static InputMode GetMode()
	{
		return _mode;
	}
}
