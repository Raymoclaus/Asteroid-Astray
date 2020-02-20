using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputHandlerSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/Input Context")]
	public class InputContext : ScriptableObject
	{
		public string contextName;

		private List<string> actions = new List<string>();
		public List<string> Actions
		{
			get
			{
				if (actions == null || actions.Count == 0)
				{
					RefreshActionList();
				}

				return actions;
			}
		}
		private List<GameAction> inputActions = new List<GameAction>();

		public bool IsValidAction(string action) => GetIndexOfAction(action) != -1;

		public int GetIndexOfAction(string action)
		{
			if (inputActions == null || inputActions.Count == 0)
			{
				RefreshActionList();
			}

			if (inputActions == null || inputActions.Count == 0) return -1;

			for (int i = 0; i < inputActions.Count; i++)
			{
				if (string.Compare(inputActions[i].ActionName, action) == 0) return i;
			}

			return -1;
		}

		private void RefreshActionList()
		{
			//clear lists
			inputActions.Clear();
			actions.Clear();

			//find all actions with this context listed as its intended context
			GameAction[] validActions = Resources.LoadAll<GameAction>(string.Empty)
				.Where(t => t.IntendedContext == this).ToArray();

			//add the found actions to this context's action list
			inputActions.AddRange(validActions);

			//keep a list of strings too
			for (int i = 0; i < inputActions.Count; i++)
			{
				actions.Add(inputActions[i].ActionName);
			}
		}
	}
}
