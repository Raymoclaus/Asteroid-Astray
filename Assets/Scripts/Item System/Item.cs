public static class Item
{
	public enum Type
	{
		Blank,
		Stone,
		Corvorite
	}

	public const int MAX_RARITY = 10;

	public static int TypeRarity(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.Stone: return 1;
			case Type.Corvorite: return 5;

			default: return 1;
		}
	}

	public static int StackLimit(Type type)
	{
		switch (type)
		{
			case Type.Blank: return 0;
			case Type.Stone: return 100;
			case Type.Corvorite: return 5;

			default: return 99;
		}
	}
}