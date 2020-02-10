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

		public string TabName
		{
			get => tabName;
			set => tabName = value;
		}

		public int PreferredTabIndex
		{
			get => preferredTabIndex;
			set => preferredTabIndex = value;
		}

		public IMenuContent CreateCopy(Transform parent) => Instantiate(this, parent);

		public virtual void OnClose()
		{

		}

		public virtual void OnOpen()
		{

		}
	}

}