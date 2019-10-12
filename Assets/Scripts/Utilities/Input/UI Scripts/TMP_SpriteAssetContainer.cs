using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InputHandler.UI
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/TMP_SpriteAsset Container")]
	public class TMP_SpriteAssetContainer : ScriptableObject
	{
		public string action;
		public List<TMP_SpriteAsset> spriteAssets;
	}

}