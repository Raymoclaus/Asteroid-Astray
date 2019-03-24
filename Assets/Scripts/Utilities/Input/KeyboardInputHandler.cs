using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KeyboardInputHandler : ICustomInputType
{
	//set of default key bindings that cannot be changed
	private static readonly List<KeyCode> defaults = new List<KeyCode>
	{
		KeyCode.None,
		KeyCode.W,
		KeyCode.S,
		KeyCode.Mouse0,
		KeyCode.LeftShift,
		KeyCode.Tab,
		KeyCode.None,
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Space,
		KeyCode.E
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
		if (!Camera.main) return 0f;

		Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return -Vector2.SignedAngle(Vector2.up, cursorPos - refLocation);
	}

	//checks all methods of input to determine if mouse/keyboard is in use, excludes non-bound inputs
	public bool ProcessInputs()
	{
		bool inputDetected = false;

		//checks all the bindings
		for (int i = 0; i < bindings.Count; i++)
		{
			KeyCode kb = bindings[i];
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

	public void ChangeKeyBinding(InputAction key, KeyCode newVal)
	{
		int index = (int)key;
		if (index < 0 || index >= bindings.Count)
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

	public float GetInput(InputAction key)
	{
		return Input.GetKey(GetBinding(key)) ? 1f : 0f;
	}

	public float GetInputDown(InputAction key)
	{
		return Input.GetKeyDown(GetBinding(key)) ? 1f : 0f;
	}

	public float GetInputUp(InputAction key)
	{
		return Input.GetKeyUp(GetBinding(key)) ? 1f : 0f;
	}

	public KeyCode GetBinding(InputAction key)
	{
		int index = (int)key;
		if (index < 0 || index >= bindings.Count)
		{
			Debug.LogWarning(string.Format("Action: {0}, does not exist.", key));
			return KeyCode.None;
		}
		return bindings[index];
	}
}
