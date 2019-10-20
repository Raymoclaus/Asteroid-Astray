using CustomDataTypes;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AsteroidSprites")]
public class AsteroidSprites : ScriptableObject
{
	public NestedSpriteArray[] asteroidSprites;
	public Sprite[] debris, dust;
}
