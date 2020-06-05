using UnityEngine;

namespace TabbedMenuSystem
{
	public interface IMenuContent
	{
		CanvasGroup CGroup { get; }
		string TabName { get; }
		int PreferredTabIndex { get; }
		void OnClose();
		void OnOpen();
		IMenuContent CreateCopy(Transform parent);
	}
}