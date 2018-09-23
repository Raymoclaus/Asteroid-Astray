using UnityEngine;

public static class Item
{
	public enum Type
	{
		Blank,
		Corvorite,
		Stone
	}

	public static Sprite[] sprites;

	public const int MAX_RARITY = 10;

	public static int TypeRarity(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.Corvorite: return 5;
			case Type.Stone: return 1;

			default: return 1;
		}
	}

	public static int StackLimit(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.Corvorite: return 5;
			case Type.Stone: return 100;

			default: return 99;
		}
	}

	public static string ItemDescription(Type type)
	{
		switch (type)
		{
			case Type.Blank: return string.Empty;
			case Type.Corvorite: return string.Empty;
			case Type.Stone: return "Charged with two counts of murder by the Avian Court of Caw.";

			default: return string.Empty;
		}
	}
}