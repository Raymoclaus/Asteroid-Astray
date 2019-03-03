using System.Collections.Generic;
using UnityEngine;

public interface ICustomInputType
{
	bool ProcessInputs();
	void ChangeKeyBinding(InputHandler.InputAction key, KeyCode newVal);
	void ChangeAllKeyBindings(List<KeyCode> keys);
	List<KeyCode> GetDefaults();
	float GetInput(InputHandler.InputAction key);
	float GetInputUp(InputHandler.InputAction key);
	float GetInputDown(InputHandler.InputAction key);
	KeyCode GetBinding(InputHandler.InputAction key);
}
