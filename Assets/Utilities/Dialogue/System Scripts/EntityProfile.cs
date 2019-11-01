using UnityEngine;

namespace DialogueSystem
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Entity Profile")]
	public class EntityProfile : ScriptableObject
	{
		public string entityName;
		public Sprite face;
		public AudioClip chatTone;
	}
}
