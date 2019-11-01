using System;
using UnityEngine;
using System.Collections.Generic;
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
		private static List<InputIconSO> modeIcons = new List<InputIconSO>();

		private void Awake()
		{
			InputManager.InputModeChanged += UpdateIcon;
			UpdateIcon();
		}

		private void OnDestroy()
		{
			InputManager.InputModeChanged -= UpdateIcon;
		}

		private void GetIconSoReferences()
		{
			List<InputIconSO> so = Resources.LoadAll<InputIconSO>("")
				.Where(t => !IsDuplicate(t)).ToList();
			modeIcons.AddRange(so);
		}

		private bool IsDuplicate(InputIconSO inputIconSo)
		{
			for (int i = 0; i < modeIcons.Count; i++)
			{
				if (modeIcons[i].inputMode == inputIconSo.inputMode) return true;
			}
			return false;
		}

		private void UpdateIcon()
		{
			textMesh = textMesh ?? GetComponent<TextMeshProUGUI>();
			Func<string, TMP_SpriteAssetContainer> getContainer = action => GetCurrentSpriteContainer(action);
			string s = StringFormatter.ConvertActionTagsToRichText(text, GetCurrentIconSet(), getContainer);
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
			ContextualInputIconContainer container = Resources.LoadAll<ContextualInputIconContainer>("")
				.Where(t => t.context == context).First();
			iconContainers.Add(container);
			return container.GetContainer(action);
		}

		private InputIconSO GetCurrentIconSet()
			=> GetIconSet(InputManager.GetMode());

		private InputIconSO GetIconSet(InputMode mode)
		{
			GetIconSoReferences();
			for (int i = 0; i < modeIcons.Count; i++)
			{
				InputIconSO so = modeIcons[i];
				if (so.inputMode == mode) return so;
			}
			return null;
		}
	}
}