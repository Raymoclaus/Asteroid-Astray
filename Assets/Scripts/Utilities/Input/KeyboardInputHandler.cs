using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utilities.Input
{
	public class KeyboardInputHandler : ICustomInputType
	{
		//set of default key bindings that cannot be changed
		private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>
		{
			{"MoveLeft", "a"},
			{"MoveRight", "d"},
			{"MoveUp", "w"},
			{"MoveDown", "s"}
		};
		//set of key bindings that can be changed
		private readonly Dictionary<string, string> _bindings = new Dictionary<string, string>();
		
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

			_prevMousePos = UnityEngine.Input.mousePosition;
		}

		private void SetToDefaults()
		{
			_bindings.Clear();
			foreach (string val in _defaults.Values)
			{
				_bindings.Add(val, val);
			}
		}

		public static float GetLookDirection(Vector2 refLocation)
		{
			Vector2 cursorPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
			float angle = Vector2.Angle(Vector2.up, cursorPos - refLocation);
			if (cursorPos.x < refLocation.x)
			{
				angle = 180f + (180f - angle);
			}

			return angle;
		}

		//checks all methods of input to determine if mouse/keyboard is in use, excludes non-bound inputs
		public bool ProcessInputs()
		{
			bool inputDetected = false;

			//checks all the bindings
			foreach (string kb in _bindings.Values)
			{
				if (!UnityEngine.Input.GetKey(kb)) continue;
				inputDetected = true;
				break;
			}

			//checks if the mouse has moved
			if (_prevMousePos == (Vector2) UnityEngine.Input.mousePosition) return inputDetected;
			
			//update mouse check variable
			_prevMousePos = UnityEngine.Input.mousePosition;
			//return true because mouse has moved
			return true;
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
			return UnityEngine.Input.GetKey(GetBinding(key)) ? 1f : 0f;
		}

		private string GetBinding(string key)
		{
			return _bindings[_defaults[key]];
		}
	}
}
