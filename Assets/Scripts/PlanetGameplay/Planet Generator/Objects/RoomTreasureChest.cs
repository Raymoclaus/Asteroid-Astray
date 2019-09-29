[System.Serializable]
public class RoomTreasureChest : RoomObject
{
	public bool IsOpen { get; set; }
	public bool IsLocked { get; set; }

	public RoomTreasureChest(bool locked)
	{
		IsLocked = locked;
	}

	public void Unlock()
	{
		IsLocked = false;
		IsOpen = true;
	}

	public override ObjType GetObjectType() => ObjType.TreasureChest;

	public override string ObjectName => "Treasure Chest";
}
