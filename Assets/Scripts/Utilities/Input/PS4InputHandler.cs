using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Utilities.Input
{
	public class Ps4InputHandler : ICustomInputType
	{
		//set of default key bindings that cannot be changed
		private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>
		{
			{"MoveHorizontal", "J_LeftHorizontalAxis"},
			{"MoveVertical", "J_LeftVerticalAxis"},
			{"LookHorizontal", "J_RightHorizontalAxis"},
			{"LookVertical", "J_RightVerticalAxis"}
		};
		//set of key bindings that can be changed
		private readonly Dictionary<string, string> _bindings = new Dictionary<string, string>();

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
			_bindings.Clear();
			foreach (string val in _defaults.Values)
			{
				_bindings.Add(val, val);
			}
		}

		public static float GetLookDirection()
		{
			Vector2 axisInput = new Vector2(
				UnityEngine.Input.GetAxisRaw("J_RightHorizontalAxis"),
				UnityEngine.Input.GetAxisRaw("J_RightVerticalAxis"));
			
			//if the control is not being used then return a non-usable value
			if (Mathf.Approximately(axisInput.x, 0f) && Mathf.Approximately(axisInput.y, 0f))
				return float.PositiveInfinity;
			
			float angle = Vector2.Angle(Vector2.up, axisInput);
			if (axisInput.x < 0f)
			{
				angle = 180f + (180f - angle);
			}
			return angle;
		}

		//checks all methods of input to determine if ps4 controller is in use, excludes non-bound inputs
		public bool ProcessInputs()
		{
			//checks all the bindings
			foreach (string kb in _bindings.Values)
			{
				if (kb.Contains("button"))
				{
					if (UnityEngine.Input.GetKey(kb))
						return true;
					
					continue;
				}

				if (!kb.Contains("Axis")) continue;
				if (!Mathf.Approximately(UnityEngine.Input.GetAxisRaw(kb), 0f))
					return true;
			}

			return false;
		}

		public void ChangeKeyBinding(string key, string newVal)
		{
			_bindings[key] = newVal;
		}

		public void ChangeAllKeyBindings(Dictionary<string, string> keys)
		{
			_bindings.Clear();
			foreach (KeyValuePair<string, string> val in keys)
			{
				_bindings.Add(_defaults[val.Key], val.Value);
			}
		}

		public Dictionary<string, string> GetDefaults()
		{
			return _defaults;
		}

		public float GetInput(string key)
		{
			if (GetBinding(key).Contains("button")) return UnityEngine.Input.GetKey(GetBinding(key)) ? 1f : 0f;
			return GetBinding(key).Contains("Axis") ? UnityEngine.Input.GetAxisRaw(GetBinding(key)) : 0f;
		}

		private string GetBinding(string key)
		{
			return _bindings[_defaults[key]];
		}
	}
}