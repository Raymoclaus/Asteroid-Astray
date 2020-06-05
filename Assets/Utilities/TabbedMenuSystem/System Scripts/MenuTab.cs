using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabbedMenuSystem
{
	public class MenuTab : MonoBehaviour, IMenuTab
	{
		[SerializeField] private TextMeshProUGUI textComponent;
		[SerializeField] private Canvas canvas;
		private string tabText;
		private int index, indexOfMainTab;
		[SerializeField] private Image imgRend;
		[SerializeField] private Sprite main, leftOfMain, rightOfMain;

		public event Action<IMenuTab> OnClicked;

		public int Index
		{
			get => index;
			set
			{
				index = value;
				UpdateImage();
			}
		}

		public int IndexOfMainTab
		{
			get => indexOfMainTab;
			set
			{
				indexOfMainTab = value;
				UpdateImage();
			}
		}

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

		private void UpdateImage()
		{
			if (index == indexOfMainTab)
			{
				imgRend.sprite = main;
			}
			else if (index < indexOfMainTab)
			{
				imgRend.sprite = leftOfMain;
			}
			else
			{
				imgRend.sprite = rightOfMain;
			}
		}

		public void SetIndex(int index) => Index = index;

		public void NotifyOfMainIndex(int index) => IndexOfMainTab = index;
	}
}