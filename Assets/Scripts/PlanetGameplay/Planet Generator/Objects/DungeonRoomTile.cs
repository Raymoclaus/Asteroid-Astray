using UnityEngine;

public struct DungeonRoomTile
{
	public DungeonRoomTileType type;

	public DungeonRoomTile(DungeonRoomTileType type, IntPair position)
	{
		this.type = type;
		Position = position;
	}

	public IntPair Position { get; set; }
}
