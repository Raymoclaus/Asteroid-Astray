using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Input
{
	public interface ICustomInputType
	{
		bool ProcessInputs();
		void ChangeKeyBinding(string key, string newVal);
		void ChangeAllKeyBindings(Dictionary<string, string> keys);
		Dictionary<string, string> GetDefaults();
		float GetInput(string key);
	}
}
