using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Input Icon List")]
public class InputIconSO : ScriptableObject
{
	[SerializeField] private List<InputIcon> inputIcons;
	public string ignoreSubstring;

	public Sprite GetSprite(KeyCode kc)
	{
		for (int i = 0; i < inputIcons.Count; i++)
		{
			if (inputIcons[i].keyCode == kc) return inputIcons[i].spr;
		}
		return null;
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
		bool found = false;
		for (int i = 0; i < inputIcons?.Count; i++)
		{
			if (inputIcons[i].spr == sprite)
			{
				found = true;
				break;
			}
		}
		if (!found)
		{
			InputIcon ii;
			if (ignoreSubstring == string.Empty)
			{
				ii = new InputIcon(sprite);
			}
			else
			{
				ii = new InputIcon(sprite, sprite.name.Replace(ignoreSubstring, string.Empty));
			}
			inputIcons.Add(ii);
		}
	}
}

[System.Serializable]
public struct InputIcon
{
	public Sprite spr;
	public KeyCode keyCode;

	public InputIcon(Sprite spr)
	{
		this.spr = spr;
		keyCode = 0;
		keyCode = FindkeyCode(spr.name);
	}

	public InputIcon(Sprite spr, string s)
	{
		this.spr = spr;
		keyCode = 0;
		keyCode = FindkeyCode(s);
	}

	public static KeyCode FindkeyCode(string s)
	{
		KeyCode kc;
		return System.Enum.TryParse(s, out kc) ? kc : 0;
	}
}