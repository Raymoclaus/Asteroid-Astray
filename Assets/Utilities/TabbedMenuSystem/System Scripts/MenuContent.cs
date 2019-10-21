using UnityEngine;

namespace TabbedMenuSystem
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class MenuContent : MonoBehaviour, IMenuContent
	{
		private CanvasGroup cGroup;
		[SerializeField] private string tabName;
		[SerializeField] private int preferredTabIndex;

		public CanvasGroup CGroup => cGroup != null ? cGroup
			: (cGroup = GetComponent<CanvasGroup>());

		public string TabName => tabName;

		public int PreferredTabIndex => preferredTabIndex;

		public IMenuContent CreateCopy(Transform parent) => Instantiate(this, parent);

		public virtual void OnClose()
		{

		}

		public virtual void OnOpen()
		{

		}
	}

}