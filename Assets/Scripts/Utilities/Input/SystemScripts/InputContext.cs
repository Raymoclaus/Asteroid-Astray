using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputHandler
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input Context")]
	public class InputContext : ScriptableObject
	{
		public string contextName;
		public List<string> actions;

		public string GetActionAtIndex(int index)
		{
			if (index < 0 || actions == null || actions.Count <= index) return null;
			return actions[index];
		}

		public bool IsValidAction(string action) => GetIndexOfAction(action) != -1;

		public int GetIndexOfAction(string action)
		{
			if (actions == null || actions.Count == 0) return -1;
			for (int i = 0; i < actions.Count; i++)
			{
				if (string.Compare(actions[i], action) == 0) return i;
			}
			return -1;
		}
	}
}
