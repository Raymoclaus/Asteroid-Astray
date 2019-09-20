using System.Collections.Generic;

namespace InputHandler
{
	[System.Serializable]
	public class InputCombination
	{
		public List<InputCode> inputs = new List<InputCode>();

		public void AddCode(InputCode.InputType codeType)
		{
			inputs = inputs ?? new List<InputCode>();
			inputs.Add(new InputCode(codeType));
		}

		public void RemoveLastCode() => inputs.RemoveAt(inputs.Count - 1);

		public InputCode.InputType GetInputTypeAtIndex(int index) => inputs[index].inputType;

		public bool CombinationHeld()
		{
			if (inputs == null || inputs.Count == 0) return false;

			for (int i = 0; i < inputs.Count; i++)
			{
				if (inputs[i].CodeValue() <= 0f) return false;
			}
			return true;
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
	}
}
