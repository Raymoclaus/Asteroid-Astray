using System.Collections.Generic;
using UnityEngine;

namespace InputHandlerSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/Input Method")]
	public class InputMethod : ScriptableObject
	{
		public InputMode inputMode;
		public InputContext context;
		public List<ActionCombination> combinations;

		public bool ProcessInputs()
		{
			for (int i = 0; i < combinations.Count; i++)
			{
				if (combinations[i].AnyInput()) return true;
			}
			return false;
		}

		private int GetCombinationIndex(string action)
		{
			for (int i = 0; i < combinations.Count; i++)
			{
				if (combinations[i].actionName == action) return i;
			}
			return -1;
		}

		public ActionCombination GetCombination(string action)
		{
			int index = GetCombinationIndex(action);
			if (index == -1) return null;
			if (index > combinations.Count)
			{
				Debug.Log($"No combination at index: {index}");
				return null;
			}
			return combinations[index];
		}

		public float GetInput(string action)
			=> GetCombination(action)?.CombinationInput() ?? 0f;

		public bool GetInputDown(string action)
			=> GetCombination(action)?.CombinationInputDown() ?? false;

		public bool GetInputUp(string action)
			=> GetCombination(action)?.CombinationInputUp() ?? false;

		public void ResetToDefaults()
		{
			for (int i = 0; i < combinations.Count; i++)
			{
				combinations[i].ResetToDefault();
			}
		}

		public void SetBinding(int index, InputCombination newCombination)
		{
			combinations[index].SetCurrentCombination(newCombination);
		}

		public void SetBinding(string action, InputCombination newCombination)
		{
			int index = GetCombinationIndex(action);
			if (index == -1)
			{
				Debug.LogWarning($"Action: {action}, does not exist.");
				return;
			}
			SetBinding(index, newCombination);
		}

		public void SetAllBindings(List<InputCombination> newCombinations)
		{
			if (newCombinations.Count != combinations.Count) return;
			for (int i = 0; i < combinations.Count; i++)
			{
				combinations[i].SetCurrentCombination(newCombinations[i]);
			}
		}

		public void UpdateInputs()
		{
			while (combinations.Count < context.Actions.Count)
			{
				combinations.Add(new ActionCombination());
			}

			combinations.RemoveRange(context.Actions.Count,
				combinations.Count - context.Actions.Count);

			for (int i = combinations.Count - 1; i >= 0; i--)
			{
				string action = combinations[i].actionName;
				int index = context.GetIndexOfAction(action);
				if (index != -1) continue;
				combinations.RemoveAt(i);
			}

			List<string> actions = context.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				if (ContainsAction(actions[i])) continue;
				ActionCombination newCombination = new ActionCombination();
				newCombination.actionName = actions[i];
				combinations.Add(newCombination);
				Debug.Log(actions[i] + " action added");
			}
		}

		private bool ContainsAction(string action)
			=> GetCombinationIndex(action) != -1;

		public bool MatchesContextActionPool
		{
			get
			{
				if (context.Actions.Count != combinations.Count) return false;
				for (int i = 0; i < context.Actions.Count; i++)
				{
					if (GetCombinationIndex(context.Actions[i]) == -1) return false;
				}
				return true;
			}
		}
	}
}
