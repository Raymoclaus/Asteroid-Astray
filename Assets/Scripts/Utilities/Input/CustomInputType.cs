using System.Collections.Generic;
using UnityEngine;

public interface ICustomInputType
{
	bool ProcessInputs();
	void ChangeKeyBinding(InputAction key, KeyCode newVal);
	void ChangeAllKeyBindings(List<KeyCode> keys);
	List<KeyCode> GetDefaults();
	float GetInput(InputAction key);
	float GetInputUp(InputAction key);
	float GetInputDown(InputAction key);
	KeyCode GetBinding(InputAction key);
}
