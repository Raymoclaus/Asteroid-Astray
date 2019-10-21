using System;
using UnityEngine;
using TMPro;

namespace TabbedMenuSystem
{
	public class MenuTab : MonoBehaviour, IMenuTab
	{
		[SerializeField] private TextMeshProUGUI textComponent;
		[SerializeField] private Canvas canvas;
		private string tabText;

		public event Action<IMenuTab> OnClicked;

		public string TabText
		{
			get => tabText;
			set
			{
				tabText = value;
				textComponent.text = tabText;
			}
		}

		public int DrawOrder
		{
			get => canvas.sortingOrder;
			set => canvas.sortingOrder = value;
		}

		public IMenuTab CreateCopy(Transform parent)
		{
			return Instantiate(this, parent);
		}

		public void OnClick()
		{
			OnClicked?.Invoke(this);
		}
	}
}