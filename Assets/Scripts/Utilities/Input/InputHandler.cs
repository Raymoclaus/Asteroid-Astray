using UnityEngine;

public enum InputAction
{
	None,
	Go, DrillLaunch, Shoot, Boost, Pause,
	HotbarSwitch, Slot1, Slot2, Slot3, Slot4, Slot5, Slot6, Slot7, Slot8,
	ScrollDialogue, Interact
}

public static class InputHandler
{
	//possible input modes
	public enum InputMode
	{
		Keyboard, Ps4
	}

	public static string GetActionString(InputAction action)
	{
		switch (action)
		{
			default: return action.ToString();
			case InputAction.DrillLaunch: return "Drill Launch";
			case InputAction.HotbarSwitch: return "Hotbar Switch";
			case InputAction.Slot1: return "Slot 1";
			case InputAction.Slot2: return "Slot 2";
			case InputAction.Slot3: return "Slot 3";
			case InputAction.Slot4: return "Slot 4";
			case InputAction.Slot5: return "Slot 5";
			case InputAction.Slot6: return "Slot 6";
			case InputAction.Slot7: return "Slot 7";
			case InputAction.Slot8: return "Slot 8";
			case InputAction.ScrollDialogue: return "Scroll Dialogue";
		}
	}

	//current input mode
	private static InputMode _mode;
	//used to query the control scheme of a keyboard
	private static KeyboardInputHandler keyLayout = new KeyboardInputHandler();
	//used to query the control scheme of a PS4 controller
	private static Ps4InputHandler ps4Layout = new Ps4InputHandler();

	public delegate void InputModeChangedEventHandler();
	public static event InputModeChangedEventHandler InputModeChanged;
	public static void ClearEvent()
	{
		InputModeChanged = null;
	}

	private static void CheckForModeUpdate()
	{
		InputMode prevMode = _mode;

		//check current mode if input detected
		if (GetHandler().ProcessInputs()) return;

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
			InputModeChanged?.Invoke();
			Debug.LogWarning($"Input mode set to: {_mode}");
		}
	}

	//returns the input status of a given command. 0f usually means no input.
	public static float GetInput(InputAction key)
	{
		if (!System.Enum.IsDefined(typeof(InputAction), key))
		{
			Debug.LogWarning($"Action: {key}, does not exist.");
			return 0f;
		}

		//make sure we're using the right mode
		CheckForModeUpdate();

		return GetHandler().GetInput(key);
	}

	//returns the input status of a given command. 0f usually means no input.
	public static float GetInputDown(InputAction key)
	{
		if (!System.Enum.IsDefined(typeof(InputAction), key))
		{
			Debug.LogWarning($"Action: {key}, does not exist.");
			return 0f;
		}

		//make sure we're using the right mode
		CheckForModeUpdate();

		return GetHandler().GetInputDown(key);
	}

	//returns the input status of a given command. 0f usually means no input.
	public static float GetInputUp(InputAction key)
	{
		int index = (int)key;
		if (!System.Enum.IsDefined(typeof(InputAction), index))
		{
			Debug.LogWarning($"Action: {key}, does not exist.");
			return 0f;
		}

		//make sure we're using the right mode
		CheckForModeUpdate();

		return GetHandler().GetInputUp(key);
	}

	public static KeyCode GetBinding(InputAction key)
	{
		return GetHandler().GetBinding(key);
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

	private static ICustomInputType GetHandler()
	{
		switch (_mode)
		{
			case InputMode.Keyboard: return keyLayout;
			case InputMode.Ps4: return ps4Layout;
			default: return null;
		}
	}
}
