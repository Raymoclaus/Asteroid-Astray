using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class InputIconTextMesh : MonoBehaviour
{
	private TextMeshProUGUI textMesh;
	private static List<SpriteAssetInputActionPair> spriteAssetActions = new List<SpriteAssetInputActionPair>(20);
	public InputHandler.InputAction action;
	public string text;
	private static InputIconSO keyboardIcons, ps4Icons;
	private const string keyboardIconsString = "Keyboard Icons", ps4IconsString = "Ps4 Icons";
	private const string spriteAssetString = " TmpSpriteAsset";

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
		InputIconSO iconSet = GetIconSet();
		if (iconSet == null) return;
		KeyCode kc = InputHandler.GetBinding(action);
		textMesh = textMesh ?? GetComponent<TextMeshProUGUI>();
		TMP_SpriteAsset tmpSpriteAsset = GetSpriteAsset();
		tmpSpriteAsset.spriteSheet = iconSet.GetSprite(kc).texture;
		tmpSpriteAsset.material.mainTexture = tmpSpriteAsset.spriteSheet;
		textMesh.spriteAsset = null;
		textMesh.SetText(string.Format(text, action));
		StartCoroutine(blah(textMesh, tmpSpriteAsset));
	}

	private IEnumerator blah(TextMeshProUGUI t, TMP_SpriteAsset ts)
	{
		yield return null;
		t.spriteAsset = ts;
	}

	private TMP_SpriteAsset GetSpriteAsset()
	{
		for (int i = 0; i < spriteAssetActions.Count; i++)
		{
			if (spriteAssetActions[i].action == action)
			{
				return spriteAssetActions[i].tmpSpriteAsset;
			}
		}
		TMP_SpriteAsset newAsset = Resources.Load<TMP_SpriteAsset>($"Sprite Assets/{action}{spriteAssetString}");
		spriteAssetActions.Add(new SpriteAssetInputActionPair(newAsset, action));
		return newAsset;
	}

	private InputIconSO GetIconSet()
	{
		GetIconSoReferences();
		switch (InputHandler.GetMode())
		{
			case InputHandler.InputMode.Keyboard: return keyboardIcons;
			case InputHandler.InputMode.Ps4: return ps4Icons;
			default: return null;
		}
	}

	private struct SpriteAssetInputActionPair
	{
		public TMP_SpriteAsset tmpSpriteAsset;
		public InputHandler.InputAction action;

		public SpriteAssetInputActionPair(TMP_SpriteAsset tmpSpriteAsset, InputHandler.InputAction action)
		{
			this.tmpSpriteAsset = tmpSpriteAsset;
			this.action = action;
		}
	}
}