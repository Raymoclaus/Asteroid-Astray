using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InputHandlerSystem.UI
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/TMP_SpriteAsset Container")]
	public class TMP_SpriteAssetContainer : ScriptableObject
	{
		public InputAction inputAction;
		public List<TMP_SpriteAsset> spriteAssets;
	}
}