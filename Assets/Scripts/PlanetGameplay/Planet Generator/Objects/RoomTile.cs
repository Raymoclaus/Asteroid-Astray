using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomTile
{
	public enum TileType
	{
		Floor,
		Wall
	}

	public Vector2Int position;
	public TileType type;

	public RoomTile(int x, int y, TileType type)
	{
		position = new Vector2Int(x, y);
		this.type = type;
	}
}
