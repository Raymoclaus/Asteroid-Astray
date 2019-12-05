using UnityEngine;

namespace DialogueSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Entity Profile")]
	public class CharacterProfile : ScriptableObject
	{
		public string characterName;
		public Sprite face;
		public AudioClip chatTone;
	}
}
