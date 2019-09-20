using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InputHandler;

[CreateAssetMenu(menuName = "Scriptable Objects/Input Icon List")]
public class InputIconSO : ScriptableObject
{
	public InputMode inputMode;
	[SerializeField] private List<InputIcon> inputIcons;
	[SerializeField] private string ignoreSubstring;

	public Sprite GetSprite(InputCode key)
		=> inputIcons.Where(t => t.input.Equals(key)).First().sprite;

	public List<Sprite> GetSprites(InputCombination combo)
	{
		List<Sprite> sprites = new List<Sprite>();
		for (int i = 0; i < inputIcons.Count; i++)
		{
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

[System.Serializable]
public struct InputIcon
{
	public Sprite sprite;
	public InputCode input;

	public InputIcon(Sprite sprite, string ignoreSubstring = "")
	{
		this.sprite = sprite;
		input = new InputCode(InputCode.InputType.Button);
		System.Enum.TryParse(sprite.name.Replace(ignoreSubstring, string.Empty), out input.buttonCode);
	}
}