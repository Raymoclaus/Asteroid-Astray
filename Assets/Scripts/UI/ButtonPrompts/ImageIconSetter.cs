using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageIconSetter : MonoBehaviour
{
	private Image image;
	public InputHandler.InputAction action;
	private static InputIconSO keyboardIcons, ps4Icons;
	private const string keyboardIconsString = "Keyboard Icons", ps4IconsString = "Ps4 Icons";

	private void Awake()
	{
		InputHandler.InputModeChanged += UpdateIcon;
		UpdateIcon();
	}

	private void GetIconSoReferences()
	{
		keyboardIcons = keyboardIcons ?? GetInputIconSO(keyboardIconsString);
		ps4Icons = ps4Icons ?? GetInputIconSO(ps4IconsString);
	}

	private InputIconSO GetInputIconSO(string name)
	{
		return Resources.Load<InputIconSO>(name);
	}

	private void UpdateIcon()
	{
		GetIconSoReferences();
		InputIconSO iconSet = GetIconSet();
		if (iconSet == null) return;
		KeyCode kc = InputHandler.GetBinding(action);
		image = image ?? GetComponent<Image>();
		image.sprite = iconSet.GetSprite(kc);
	}

	private InputIconSO GetIconSet()
	{
		switch (InputHandler.GetMode())
		{
			case InputHandler.InputMode.Keyboard: return keyboardIcons;
			case InputHandler.InputMode.Ps4: return ps4Icons;
			default: return null;
		}
	}
}