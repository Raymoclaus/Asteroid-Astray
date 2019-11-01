using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputHandlerSystem
{
	[Serializable]
	public class InputCombination
	{
		public List<InputCode> inputs = new List<InputCode>();

		public bool IsValid
		{
			get
			{
				for (int i = 0; i < inputs.Count; i++)
				{
					if (!inputs[i].IsValid) return false;
				}
				return inputs.Count > 0;
			}
		}

		public void AddCode()
		{
			inputs = inputs ?? new List<InputCode>();
			inputs.Add(new InputCode());
		}

		public void AddCode(InputCode.InputType codeType)
		{
			inputs = inputs ?? new List<InputCode>();
			inputs.Add(new InputCode(codeType));
		}

		public void RemoveAtIndex(int index)
		{
			if (index < 0 || index >= inputs.Count) return;
			inputs.RemoveAt(index);
		}

		public void RemoveLastCode() => inputs.RemoveAt(inputs.Count - 1);

		public InputCode.InputType GetInputTypeAtIndex(int index) => inputs[index].inputType;

		public float CombinationInput()
		{
			if (inputs == null || inputs.Count == 0) return 0f;
			float inputValue = Mathf.Infinity;
			for (int i = 0; i < inputs.Count; i++)
			{
				float codeValue = inputs[i].CodeValue();
				if (codeValue <= 0f) return 0f;
				inputValue = Mathf.Min(inputValue, codeValue);
			}
			return inputValue;
		}

		public bool CombinationInputDown()
		{
			if (inputs == null || inputs.Count == 0) return false;
			if (!ContainsButtonInput) return false;
			for (int i = 0; i < inputs.Count; i++)
			{
				switch (inputs[i].inputType)
				{
					default: return false;
					case InputCode.InputType.Button:
						if (!inputs[i].CodePressed()) return false;
						break;
					case InputCode.InputType.Axis:
						if (inputs[i].CodeValue() == 0f) return false;
						break;
				}
			}
			return true;
		}

		public bool CombinationInputUp()
		{
			if (inputs == null || inputs.Count == 0) return false;
			if (!ContainsButtonInput) return false;
			for (int i = 0; i < inputs.Count; i++)
			{
				switch (inputs[i].inputType)
				{
					default: return false;
					case InputCode.InputType.Button:
						if (!inputs[i].CodeReleased()) return false;
						break;
					case InputCode.InputType.Axis:
						if (inputs[i].CodeValue() == 0f) return false;
						break;
				}
			}
			return true;
		}

		public bool ContainsButtonInput
		{
			get
			{
				if (inputs == null) return false;
				for (int i = 0; i < inputs.Count; i++)
				{
					if (inputs[i].inputType == InputCode.InputType.Button) return true;
				}
				return false;
			}
		}

		public bool AnyInput()
		{
			for (int i = 0; i < inputs.Count; i++)
			{
				if (inputs[i].CodeValue() > 0f) return true;
			}
			return false;
		}

		public bool Contains(InputCode code)
		{
			for (int i = 0; i < inputs.Count; i++)
			{
				if (inputs[i].Equals(code)) return true;
			}
			return false;
		}

		public override string ToString()
		{
			string s = $"Combination Count: {inputs.Count}\n";
			for (int i = 0; i < inputs.Count; i++)
			{
				s += inputs[i];
			}
			return s;
		}
	}
}
