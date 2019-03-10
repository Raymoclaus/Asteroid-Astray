using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Ps4InputHandler : ICustomInputType
{
	//https://www.google.com/search?q=ps4+input+map&rlz=1C1CHBF_en-GBAU819AU819&source=lnms&tbm=isch&sa=X&ved=0ahUKEwikzP_7lvTgAhVSdCsKHUXwDSQQ_AUIDigB&biw=1920&bih=937#imgrc=kNz76yy98oC0CM:
	//set of default key bindings that cannot be changed
	private static readonly List<KeyCode> defaults = new List<KeyCode>
	{
		KeyCode.JoystickButton7,
		KeyCode.JoystickButton0,
		KeyCode.JoystickButton5,
		KeyCode.JoystickButton1,
		KeyCode.JoystickButton9,
		KeyCode.JoystickButton4,
		KeyCode.UpArrow,
		KeyCode.LeftArrow,
		KeyCode.DownArrow,
		KeyCode.RightArrow,
		KeyCode.UpArrow,
		KeyCode.LeftArrow,
		KeyCode.DownArrow,
		KeyCode.RightArrow,
		KeyCode.JoystickButton1,
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
		for (int i = 0; i < bindings.Count; i++)
		{
			KeyCode kb = bindings[i];
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

	public void ChangeKeyBinding(InputHandler.InputAction key, KeyCode newVal)
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

	public float GetInput(InputHandler.InputAction key)
	{
		return Input.GetKey(GetBinding(key)) ? 1f : 0f;
	}

	public float GetInputDown(InputHandler.InputAction key)
	{
		return Input.GetKeyDown(GetBinding(key)) ? 1f : 0f;
	}

	public float GetInputUp(InputHandler.InputAction key)
	{
		return Input.GetKeyUp(GetBinding(key)) ? 1f : 0f;
	}

	public KeyCode GetBinding(InputHandler.InputAction key)
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