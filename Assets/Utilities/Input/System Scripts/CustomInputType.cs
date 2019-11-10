using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace InputHandlerSystem
{
	public abstract class CustomInputHandler
	{
		private static InputMethod[] inputMethods;
		private static InputMethod[] InputMethods
			=> inputMethods != null
			? inputMethods
			: (inputMethods = Resources.LoadAll<InputMethod>("Input Contexts"));

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

		public float GetInput(string key, InputContext context)
			=> GetInputMethod(context)?.GetInput(key) ?? 0f;

		public bool GetInputDown(string key, InputContext context)
			=> GetInputMethod(context)?.GetInputDown(key) ?? false;

		public bool GetInputUp(string key, InputContext context)
			=> GetInputMethod(context)?.GetInputUp(key) ?? false;

		public ActionCombination GetBinding(string key, InputContext context)
			=> GetInputMethod(context)?.GetCombination(key);

		public InputMethod GetInputMethod(InputContext context)
		{
			if (context == null)
			{
				Debug.Log("No context given");
				return null;
			}
			
			if (InputMethods.Length == 0) return null;
			
			for (int i = 0; i < InputMethods.Length; i++)
			{
				InputMethod method = InputMethods[i];
				if (method.context == context
					&& method.inputMode == GetInputMode()) return method;
			}
			return null;
		}

		public void SetAllBindingsToDefaults()
		{
			IEnumerable<InputMethod> methods
				= InputMethods.Where(t => t.inputMode == GetInputMode());
			foreach (InputMethod method in methods)
			{
				method.ResetToDefaults();
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
