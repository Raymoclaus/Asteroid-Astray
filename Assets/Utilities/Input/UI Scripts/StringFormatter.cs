using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InputHandlerSystem.UI
{
	public class StringFormatter
	{
		public static string ConvertActionTagsToRichText(string input, InputIconSO iconSet, Func<string, TMP_SpriteAssetContainer> getContainer)
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

				if (iconSet == null) continue;

				List<Sprite> sprites = iconSet.GetSprites(inputCombo);
				TMP_SpriteAssetContainer container = getContainer(action);
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
	}
}
