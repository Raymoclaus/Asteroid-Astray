[System.Serializable]
public abstract class RoomObject
{
	private IntPair position;
	[System.NonSerialized] protected Room room;

	public delegate void PositionUpdatedEventHandler(IntPair position);
	public event PositionUpdatedEventHandler OnPositionUpdated;

	public IntPair GetPosition() => position;
	public void SetPosition(IntPair position)
	{
		if (this.position == position) return;
		this.position = position;
		OnPositionUpdated?.Invoke(position);
	}

	public virtual ObjType GetObjectType() => ObjType.None;

	public virtual string ObjectName => "Object";

	public enum ObjType
	{
		None,
		Lock,
		Key,
		ExitTrigger,
		LandingPad,
		TileLight,
		PushableBlock,
		Player,
		GreenGroundButton,
		RedGroundButton,
		Dummy,
		Spooder,
		Charger,
		Boomee,
		Smokey,
		Gargantula,
		TreasureChest
	}
}
