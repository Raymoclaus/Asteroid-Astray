using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputHandler
{
	[System.Serializable]
	public class InputCode
	{
		public enum InputType
		{
			None, Button, Axis
		}

		public InputType inputType;
		public KeyCode buttonCode;
		public string axisName;
		public bool axisPositiveDirection = true;

		public InputCode(InputType type)
		{
			inputType = type;
		}

		public virtual float CodeValue()
		{
			switch (inputType)
			{
				default: return 0f;
				case InputType.Button: return Input.GetKey(buttonCode) ? 1f : 0f;
				case InputType.Axis:
					return Mathf.Clamp01(
						Input.GetAxis(axisName) * (axisPositiveDirection ? 1f : -1f));
			}
		}

		public virtual bool CodePressed()
		{
			switch (inputType)
			{
				default: return false;
				case InputType.Button: return Input.GetKeyDown(buttonCode);
			}
		}

		public virtual bool CodeReleased()
		{
			switch (inputType)
			{
				default: return false;
				case InputType.Button: return Input.GetKeyUp(buttonCode);
			}
		}

		public virtual object GetData()
		{
			switch (inputType)
			{
				default: return "";
				case InputType.Button: return buttonCode;
				case InputType.Axis: return axisName;
			}
		}

		public override string ToString() => inputType + ": " + GetData();

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is InputCode)) return false;

			InputCode other = (InputCode)obj;
			if (inputType != other.inputType) return false;

			switch (inputType)
			{
				default: return false;
				case InputType.Button: return buttonCode == other.buttonCode;
				case InputType.Axis:
					return string.Compare(axisName, other.axisName) == 0
						&& axisPositiveDirection == other.axisPositiveDirection;
			}
		}
	}
}
