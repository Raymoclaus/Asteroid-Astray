using System.Collections.Generic;
using UnityEngine;

public interface ICustomInputType
{
	bool ProcessInputs();
	void ChangeKeyBinding(string key, KeyCode newVal);
	void ChangeAllKeyBindings(List<KeyCode> keys);
	List<KeyCode> GetDefaults();
	float GetInput(string key);
}
