using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputHandler
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input Method")]
	public class InputMethod : ScriptableObject
	{
		public InputMode inputMode;
		public InputContext context;
		public List<InputCombination> currentCombinations;
		public List<InputCombination> defaultCombinations;

		public void SetCombinationsLength(int length)
		{
			while (currentCombinations.Count < length)
			{
				currentCombinations.Add(new InputCombination());
			}
			
			currentCombinations.RemoveRange(length, currentCombinations.Count - length);
			SetDefaultCombinationsLength(length);
		}

		private void SetDefaultCombinationsLength(int length)
		{
			while (defaultCombinations.Count < length)
			{
				defaultCombinations.Add(new InputCombination());
			}

			defaultCombinations.RemoveRange(length, defaultCombinations.Count - length);
		}

		public bool ProcessInputs()
		{
			for (int i = 0; i < currentCombinations.Count; i++)
			{
				if (currentCombinations[i].AnyInput()) return true;
			}
			return false;
		}

		private int GetCombinationIndex(string action)
		{
			int index = context.GetIndexOfAction(action);
			if (index == -1)
			{
				Debug.LogWarning($"Action: {action}, does not exist.");
			}
			return index;
		}

		public InputCombination GetCombination(string action)
		{
			int index = GetCombinationIndex(action);
			if (index == -1) return null;
			return currentCombinations[index];
		}

		public bool GetInput(string action)
			=> GetCombination(action)?.CombinationHeld() ?? false;

		public void ResetToDefaults()
			=> currentCombinations = new List<InputCombination>(defaultCombinations);

		public void SetBinding(int index, InputCombination newCombination)
			=> currentCombinations[index] = newCombination;

		public void SetBinding(string action, InputCombination newCombination)
		{
			int index = context.GetIndexOfAction(action);
			if (index == -1)
			{
				Debug.LogWarning($"Action: {action}, does not exist.");
				return;
			}
			currentCombinations[index] = newCombination;
		}

		public void SetAllBindings(List<InputCombination> combinations)
			=> currentCombinations = combinations;
	}
}
