using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KeyboardInputHandler : ICustomInputType
{
	//set of default key bindings that cannot be changed
	private readonly List<KeyCode> defaults = new List<KeyCode>
	{
		KeyCode.W,
		KeyCode.S,
		KeyCode.Mouse0,
		KeyCode.LeftShift,
		KeyCode.Tab
	};
	//set of key bindings that can be changed
	private List<KeyCode> bindings = new List<KeyCode>();

	//used for checking if the mouse has moved
	private Vector2 _prevMousePos;

	public KeyboardInputHandler()
	{
		//check if a keyboard control scheme already exists and use that
		if (File.Exists("keyBinds.txt"))
		{

		}
		else
		{
			//otherwise sets the bindings to default values
			SetToDefaults();
		}

		_prevMousePos = Input.mousePosition;
	}

	private void SetToDefaults()
	{
		bindings = defaults;
	}

	public static float GetLookDirection(Vector2 refLocation)
	{
		Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return -Vector2.SignedAngle(Vector2.up, cursorPos - refLocation);
	}

	//checks all methods of input to determine if mouse/keyboard is in use, excludes non-bound inputs
	public bool ProcessInputs()
	{
		bool inputDetected = false;

		//checks all the bindings
		foreach (KeyCode kb in bindings)
		{
			if (!Input.GetKey(kb)) continue;
			inputDetected = true;
			break;
		}

		//checks if the mouse has moved
		if (_prevMousePos == (Vector2)Input.mousePosition) return inputDetected;

		//update mouse check variable
		_prevMousePos = Input.mousePosition;
		//return true because mouse has moved
		return true;
	}

	public void ChangeKeyBinding(string key, KeyCode newVal)
	{
		int index = InputHandler.GetKeyIndex(key);
		if (index == -1)
		{
			Debug.LogWarning(string.Format("Action: {0}, does not exist.", key));
			return;
		}
		bindings[index] = newVal;
	}

	public void ChangeAllKeyBindings(List<KeyCode> keys)
	{
		bindings = keys;
	}

	public List<KeyCode> GetDefaults()
	{
		return defaults;
	}

	public float GetInput(string key)
	{
		return Input.GetKey(GetBinding(key)) ? 1f : 0f;
	}

	public float GetInputDown(string key)
	{
		return Input.GetKeyDown(GetBinding(key)) ? 1f : 0f;
	}

	public float GetInputUp(string key)
	{
		return Input.GetKeyUp(GetBinding(key)) ? 1f : 0f;
	}

	private KeyCode GetBinding(string key)
	{
		int index = InputHandler.GetKeyIndex(key);
		if (index == -1)
		{
			Debug.LogWarning(string.Format("Action: {0}, does not exist.", key));
			return KeyCode.None;
		}
		return bindings[index];
	}
}
