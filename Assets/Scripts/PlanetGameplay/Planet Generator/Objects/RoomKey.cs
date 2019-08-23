public class RoomKey : RoomObject
{
	public enum KeyColour
	{
		Blue,
		Red,
		Yellow,
		Green
	}

	public RoomKey(KeyColour colour)
	{
		this.colour = colour;
	}

	public KeyColour colour;

	public override ObjType GetObjectType() => ObjType.Key;

	public static Item.Type ConvertToItemType(KeyColour col)
	{
		switch (col)
		{
			default: return Item.Type.BlueKey;
			case KeyColour.Blue: return Item.Type.BlueKey;
			case KeyColour.Red: return Item.Type.RedKey;
			case KeyColour.Yellow: return Item.Type.YellowKey;
			case KeyColour.Green: return Item.Type.GreenKey;
		}
	}
}
