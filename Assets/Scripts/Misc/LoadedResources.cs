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
	public ItemSprites itemSprites;

	public Sprite GetItemSprite(Item.Type type)
	{
		switch (type)
		{
			default: return itemSprites.itemBlank;
			case Item.Type.Blank: return itemSprites.itemBlank;
			case Item.Type.Corvorite: return itemSprites.itemCorvorite;
			case Item.Type.Stone: return itemSprites.itemStone;
		}
	}

	[System.Serializable]
	public class ItemSprites
	{
		public Sprite itemBlank, itemCorvorite, itemStone;
	}
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