using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace InputHandler
{
	public abstract class CustomInputHandler
	{
		public CustomInputHandler() => LoadBindings();

		public virtual bool ProcessInputs(InputContext context)
		{
			if (context == null) return false;
			//checks all the bindings
			if (GetInputMethod(context)?.ProcessInputs() ?? false) return true;
			return false;
		}

		public virtual float GetLookAngle(Vector2 refLocation) => 0f;

		public void ChangeKeyBinding(string key, InputCombination newVal, InputContext context)
			=> GetInputMethod(context)?.SetBinding(key, newVal);

		public void ChangeAllKeyBindings(List<InputCombination> keys, InputContext context)
			=> GetInputMethod(context)?.SetAllBindings(keys);

		public List<InputCombination> GetDefaults(InputContext context)
			=> GetInputMethod(context)?.defaultCombinations;

		public float GetInput(string key, InputContext context)
			=> GetInputMethod(context)?.GetInput(key) ?? 0f;

		public bool GetInputDown(string key, InputContext context)
			=> GetInputMethod(context)?.GetInputDown(key) ?? false;

		public bool GetInputUp(string key, InputContext context)
			=> GetInputMethod(context)?.GetInputUp(key) ?? false;

		public InputCombination GetBinding(string key, InputContext context)
			=> GetInputMethod(context)?.GetCombination(key);

		public InputMethod GetInputMethod(InputContext context)
		{
			if (context == null) return null;

			InputMethod[] inputMethods = Resources.LoadAll<InputMethod>("");
			if (inputMethods.Length == 0) return null;
			
			for (int i = 0; i < inputMethods.Length; i++)
			{
				InputMethod method = inputMethods[i];
				if (method.context == context
					&& method.inputMode == GetInputMode()) return method;
			}
			return null;
		}

		public void SetAllBindingsToDefaults()
		{
			List<InputMethod> inputMethods = Resources.LoadAll<InputMethod>("")
				.Where(t => t.inputMode == GetInputMode()).ToList();

			for (int i = 0; i < inputMethods.Count; i++)
			{
				inputMethods[i].ResetToDefaults();
			}
		}

		private void LoadBindings()
		{
			//check if a ps4 control scheme already exists and use that
			if (File.Exists($"{GetInputMode()} Bindings.txt"))
			{

			}
			else
			{
				//otherwise sets the bindings to default values
				SetAllBindingsToDefaults();
			}
		}

		public virtual InputMode GetInputMode() => InputMode.None;
	}
}
