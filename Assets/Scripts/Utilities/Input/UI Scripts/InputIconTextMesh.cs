using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace InputHandler.UI
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
			string s = ReformatText(text);
			textMesh.text = s;
			if (textMesh == null)
			{
				Debug.Log("Text mesh is null");
				return;
			}
			textMesh.gameObject.SetActive(false);
			textMesh.gameObject.SetActive(true);
		}

		public string GetText() => text;

		public void SetText(string s)
		{
			text = s;
			UpdateIcon();
		}

		private string ReformatText(string input)
		{
			if (input == string.Empty) return input;
			string s = input;

			List<string> actions = InputManager.GetCurrentActions();
			for (int i = 0; i < actions?.Count; i++)
			{
				string action = actions[i];
				string check;
				ActionCombination inputCombo = InputManager.GetBinding(action);
				if (inputCombo == null)
				{
					Debug.Log($"No ActionCombination found for {action}");
					return input;
				}

				check = $"[:{action}]";
				if (s.Contains(check))
				{
					s = s.Replace(check, $"<color=#00FFFF>{action}</color>");
				}

				InputIconSO iconSet = GetCurrentIconSet();
				if (iconSet == null) continue;

				List<Sprite> sprites = iconSet.GetSprites(inputCombo);
				TMP_SpriteAssetContainer container = GetCurrentSpriteContainer(action);
				if (container == null)
				{
					Debug.Log($"Sprite Container not found for {action}.");
					return input;
				}
				List<TMP_SpriteAsset> assets = container?.spriteAssets;

				check = $"[{action}]";
				if (s.Contains(check))
				{
					string tmpIconString = "";
					for (int j = 0; j < sprites.Count; j++)
					{
						assets[j].spriteSheet = sprites[j].texture;
						assets[j].material.mainTexture = assets[j].spriteSheet;
						tmpIconString += $"<sprite=\"{assets[j].name}\" index=0>";
						if (j != sprites.Count - 1)
						{
							tmpIconString += " + ";
						}
					}
					s = s.Replace(check, $"{tmpIconString} <color=#00FFFF>{action}</color>");
				}

				check = $"[{action}:]";
				if (s.Contains(check))
				{
					string tmpIconString = "";
					for (int j = 0; j < sprites.Count; j++)
					{
						assets[j].spriteSheet = sprites[j].texture;
						assets[j].material.mainTexture = assets[j].spriteSheet;
						tmpIconString += $"<sprite=\"{assets[j].name}\" index=0>";
						if (j != sprites.Count - 1)
						{
							tmpIconString += " + ";
						}
					}
					s = s.Replace(check, tmpIconString);
				}
			}

			return s;
		}

		private TMP_SpriteAssetContainer GetCurrentSpriteContainer(string action)
			=> GetSpriteContainer(action, InputManager.CurrentContext);

		private TMP_SpriteAssetContainer GetSpriteContainer(string action, InputContext context)
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