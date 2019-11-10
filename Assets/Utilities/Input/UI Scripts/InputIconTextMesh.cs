using System;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;

namespace InputHandlerSystem.UI
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class InputIconTextMesh : MonoBehaviour
	{
		public TextMeshProUGUI textMesh;
		private static List<ContextualInputIconContainer> iconContainers
			= new List<ContextualInputIconContainer>();
		[SerializeField] [TextArea(1, 3)] private string text;

		private void Awake()
		{
			InputManager.InputModeChanged += UpdateIcon;
			UpdateIcon();
		}

		private void OnDestroy()
		{
			InputManager.InputModeChanged -= UpdateIcon;
		}

		private static InputIconSO[] modeIcons;
		private static InputIconSO[] ModeIcons
			=> modeIcons != null
				? modeIcons
				: (modeIcons = Resources.LoadAll<InputIconSO>("Input Icons"));

		private bool IsDuplicate(InputIconSO inputIconSo)
		{
			for (int i = 0; i < ModeIcons.Length; i++)
			{
				if (ModeIcons[i].inputMode == inputIconSo.inputMode) return true;
			}
			return false;
		}

		private void UpdateIcon()
		{
			if (InputManager.GetMode() == InputMode.None) return;

			InputIconSO iconSet = GetCurrentIconSet();
			textMesh = textMesh ?? GetComponent<TextMeshProUGUI>();
			Func<string, TMP_SpriteAssetContainer> getContainer = action => GetCurrentSpriteContainer(action);
			string s = StringFormatter.ConvertActionTagsToRichText(text, iconSet, getContainer);
			if (textMesh == null)
			{
				Debug.Log("Text mesh is null");
				return;
			}
			textMesh.text = s;
			textMesh.gameObject.SetActive(false);
			textMesh.gameObject.SetActive(true);
		}

		public string GetText() => text;

		public void SetText(string s)
		{
			text = s;
			UpdateIcon();
		}

		private static TMP_SpriteAssetContainer GetCurrentSpriteContainer(string action)
			=> GetSpriteContainer(action, InputManager.CurrentContext);

		private static TMP_SpriteAssetContainer GetSpriteContainer(string action, InputContext context)
		{
			for (int i = 0; i < iconContainers.Count; i++)
			{
				if (iconContainers[i].context == context)
					return iconContainers[i].GetContainer(action);
			}
			ContextualInputIconContainer container = Containers
				.Where(t => t.context == context).First();
			iconContainers.Add(container);
			return container.GetContainer(action);
		}

		private static ContextualInputIconContainer[] containers;
		private static ContextualInputIconContainer[] Containers
			=> containers != null
				? containers
				: (containers = Resources.LoadAll<ContextualInputIconContainer>("Input Icons"));

		private InputIconSO GetCurrentIconSet()
			=> GetIconSet(InputManager.GetMode());

		private InputIconSO GetIconSet(InputMode mode)
		{
			for (int i = 0; i < ModeIcons.Length; i++)
			{
				InputIconSO so = ModeIcons[i];
				if (so.inputMode == mode) return so;
			}
			return null;
		}
	}
}