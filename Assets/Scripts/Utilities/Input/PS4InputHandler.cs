using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Ps4InputHandler : ICustomInputType
{
	//set of default key bindings that cannot be changed
	private readonly List<KeyCode> defaults = new List<KeyCode>
	{
		KeyCode.Joystick1Button7,
		KeyCode.Joystick1Button0,
		KeyCode.Joystick1Button5,
		KeyCode.Joystick1Button1,
		KeyCode.Joystick1Button9
	};
	//set of key bindings that can be changed
	private List<KeyCode> bindings = new List<KeyCode>();
	//used to check if a direction (degrees) is "roughly" close to the expected value
	private const float LEEWAY = 45f;

	public Ps4InputHandler()
	{
		//check if a ps4 control scheme already exists and use that
		if (File.Exists("Ps4Binds.txt"))
		{

		}
		else
		{
			//otherwise sets the bindings to default values
			SetToDefaults();
		}
	}

	private void SetToDefaults()
	{
		bindings = defaults;
	}

	public static float GetLookDirection()
	{
		Vector2 axisInput = new Vector2(
			Input.GetAxisRaw("J_LeftHorizontalAxis"),
			Input.GetAxisRaw("J_LeftVerticalAxis"));

		//if the control is not being used then return a non-usable value
		if (Mathf.Approximately(axisInput.x, 0f) && Mathf.Approximately(axisInput.y, 0f))
			return float.PositiveInfinity;

		return -Vector2.SignedAngle(Vector2.up, axisInput);
	}

	//checks all methods of input to determine if ps4 controller is in use, excludes non-bound inputs
	public bool ProcessInputs()
	{
		//checks all the bindings
		foreach (KeyCode kb in bindings)
		{
			if (Input.GetKey(kb)) return true;
		}

		if (!Mathf.Approximately(Input.GetAxisRaw("J_LeftHorizontalAxis"), 0f))
			return true;
		if (!Mathf.Approximately(Input.GetAxisRaw("J_LeftVerticalAxis"), 0f))
			return true;
		if (!Mathf.Approximately(Input.GetAxisRaw("J_RightHorizontalAxis"), 0f))
			return true;
		if (!Mathf.Approximately(Input.GetAxisRaw("J_RightVerticalAxis"), 0f))
			return true;

		return false;
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

	private Vector2 MoveVector()
	{
		return new Vector2(GetInput("MoveHorizontal"), GetInput("MoveVertical"));
	}
}