using UnityEngine;

public class RoomTile : RoomObject
{
	public enum TileType
	{
		Floor,
		Wall
	}

	public Vector2Int position;
	public TileType type;

	public RoomTile(Vector2Int position, TileType type)
	{
		this.position = position;
		this.type = type;
	}

	public RoomTile(int posX, int posY, TileType type)
	{
		this.position = new Vector2Int(posX, posY);
		this.type = type;
	}
}
