using UnityEngine;

[System.Serializable]
public class SpriteArray
{
	//different crack levels
	public Sprite[] sprites;
}

[System.Serializable]
public class NestedSpriteArray
{
	//different shapes
	public SpriteArray[] collection;
}