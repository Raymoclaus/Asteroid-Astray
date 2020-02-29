using UnityEngine;
using UnityEngine.UI;

public class IconButtonUI : ButtonUI
{
	[SerializeField] private Image icon;

	public void SetIcon(Sprite spr)
	{
		icon.sprite = spr;
	}
}
