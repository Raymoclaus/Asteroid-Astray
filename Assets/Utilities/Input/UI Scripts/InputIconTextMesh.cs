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
		private TextMeshProUGUI textMesh;
		public TextMeshProUGUI TextMesh
			=> textMesh != null
				? textMesh
				: (textMesh = GetComponent<TextMeshProUGUI>());

		private static List<ContextualInputIconContainer> iconContainers
			= new List<ContextualInputIconContainer>();
		[SerializeField] [TextArea(1, 3)] private string text;

		private void Awake()
		{
			InputManager.OnInputModeChanged += UpdateIcon;
			InputManager.OnContextChanged += UpdateIcon;
			UpdateIcon();
		}

		private void OnDestroy()
		{
			InputManager.OnInputModeChanged -= UpdateIcon;
			InputManager.OnContextChanged -= UpdateIcon;
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
			Func<string, TMP_SpriteAssetContainer> getContainer = action => GetCurrentSpriteContainer(action);
			string s = StringFormatter.ConvertActionTagsToRichText(text, iconSet, getContainer);
			if (TextMesh == null)
			{
				Debug.Log("Text mesh is null");
				return;
			}
			TextMesh.text = s;
			TextMesh.gameObject.SetActive(false);
			TextMesh.gameObject.SetActive(true);
		}

		public string GetText() => text;

		public void SetText(string s)
		{
			text = s;
			UpdateIcon();
		}

		private static TMP_SpriteAssetContainer GetCurrentSpriteContainer(string action)
			=> GetSpriteContainer(action, InputManager.GetCurrentContext());

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