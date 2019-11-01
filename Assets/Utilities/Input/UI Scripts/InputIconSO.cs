using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputHandlerSystem.UI
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input Icon List")]
	public class InputIconSO : ScriptableObject
	{
		public InputMode inputMode;
		[SerializeField] private List<InputIcon> inputIcons;
		[SerializeField] private string ignoreSubstring;

		public Sprite GetSprite(InputCode key)
			=> inputIcons.Where(t => t.input.Equals(key)).First().sprite;

		public List<Sprite> GetSprites(ActionCombination combo)
		{
			List<Sprite> sprites = new List<Sprite>();
			for (int i = 0; i < inputIcons.Count; i++)
			{
				if (combo == null)
				{
					Debug.Log($"Invalid Action Combination: {combo}");
					return null;
				}
				if (combo.Contains(inputIcons[i].input))
				{
					sprites.Add(inputIcons[i].sprite);
				}
			}
			return sprites;
		}

		public void AddToList(List<Sprite> sprites)
		{
			for (int i = 0; i < sprites.Count; i++)
			{
				AddToList(sprites[i]);
			}
		}

		public void AddToList(Sprite sprite)
		{
			if (ContainsSprite(sprite)) return;
			inputIcons.Add(new InputIcon(sprite, ignoreSubstring));
		}

		private bool ContainsSprite(Sprite sprite)
		{
			for (int i = 0; i < inputIcons.Count; i++)
			{
				if (inputIcons[i].sprite == sprite) return true;
			}
			return false;
		}
	}
}
