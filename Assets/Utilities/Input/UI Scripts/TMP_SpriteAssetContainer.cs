using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InputHandlerSystem.UI
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Input System/TMP_SpriteAsset Container")]
	public class TMP_SpriteAssetContainer : ScriptableObject
	{
		public GameAction gameAction;
		public List<TMP_SpriteAsset> spriteAssets;
	}
}