using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputHandlerSystem
{
	[Serializable]
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

		public InputCode() { }

		public InputCode(InputType type)
		{
			inputType = type;
		}

		public bool IsValid
		{
			get
			{
				switch (inputType)
				{
					default: return false;
					case InputType.Button:
						return buttonCode != KeyCode.None;
					case InputType.Axis:
						if (axisName == string.Empty) return false;
						try
						{
							UnityEngine.Input.GetAxisRaw(axisName);
							return true;
						}
						catch (UnityException e)
						{
							Debug.Log($"Axis: {axisName} does not exist. {e}");
							return false;
						}
				}
			}
		}

		public virtual float CodeValue()
		{
			switch (inputType)
			{
				default: return 0f;
				case InputType.Button: return UnityEngine.Input.GetKey(buttonCode) ? 1f : 0f;
				case InputType.Axis:
					return Mathf.Clamp01(
						UnityEngine.Input.GetAxis(axisName) * (axisPositiveDirection ? 1f : -1f));
			}
		}

		public virtual bool CodePressed()
		{
			switch (inputType)
			{
				default: return false;
				case InputType.Button: return UnityEngine.Input.GetKeyDown(buttonCode);
			}
		}

		public virtual bool CodeReleased()
		{
			switch (inputType)
			{
				default: return false;
				case InputType.Button: return UnityEngine.Input.GetKeyUp(buttonCode);
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

		public override string ToString()
		{
			string s = inputType.ToString();
			switch (inputType)
			{
				case InputType.Button:
					s += $": {buttonCode}";
					break;
				case InputType.Axis:
					s += $": {axisName} > Positive Axis: {axisPositiveDirection}";
					break;
			}
			return s;
		}

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

		public override int GetHashCode()
		{
			var hashCode = -1367569829;
			hashCode = hashCode * -1521134295 + inputType.GetHashCode();
			hashCode = hashCode * -1521134295 + buttonCode.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(axisName);
			hashCode = hashCode * -1521134295 + axisPositiveDirection.GetHashCode();
			hashCode = hashCode * -1521134295 + IsValid.GetHashCode();
			return hashCode;
		}
	}
}
