using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputHandlerSystem.UI
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/Contextual Icon Containers")]
	public class ContextualInputIconContainer : ScriptableObject
	{
		public InputContext context;
		public List<TMP_SpriteAssetContainer> containers;

		public TMP_SpriteAssetContainer GetContainer(string action)
		{
			IEnumerable<TMP_SpriteAssetContainer> search
				= containers.Where(t => string.Compare(t.gameAction.ActionName, action) == 0);
			if (search.Count() == 0)
			{
				Debug.LogWarning($"No sprite container for {action} found.");
			}
			return search.FirstOrDefault();
		}
	}
}
