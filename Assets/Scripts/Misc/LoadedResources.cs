using UnityEngine;

public class LoadedResources : MonoBehaviour
{
	//large, medium, small asteroids
	public NestedSpriteArray[] asteroidSprites;

	//asteroid debris
	public Sprite[] debris;

	//asteroid dust
	public Sprite[] dust;

	//item sprites
	public Sprite[] itemSprites;
}

[System.Serializable]
public struct SpriteArray
{
	//different crack levels
	public Sprite[] sprites;
}

[System.Serializable]
public struct NestedSpriteArray
{
	//different shapes
	public SpriteArray[] collection;
}