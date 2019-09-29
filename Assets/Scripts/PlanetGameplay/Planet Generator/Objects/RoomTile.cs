using UnityEngine;

[System.Serializable]
public class RoomTile : RoomObject
{
	public enum TileType
	{
		Floor,
		Wall
	}

	public IntPair position;
	public TileType type;

	public RoomTile(IntPair position, TileType type)
	{
		this.position = position;
		this.type = type;
	}

	public RoomTile(int posX, int posY, TileType type)
	{
		this.position = new IntPair(posX, posY);
		this.type = type;
	}
}
