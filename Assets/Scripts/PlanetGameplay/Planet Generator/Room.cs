using System.Collections.Generic;
using UnityEngine;

public class Room
{
	public RoomType type;
	private bool upExit, rightExit, downExit, leftExit;
	private Vector2Int upExitPos, rightExitPos, downExitPos, leftExitPos;
	private bool upLocked, rightLocked, downLocked, leftLocked;
	private RoomLock upLock, rightLock, downLock, leftLock;
	private Room upRoom, rightRoom, downRoom, leftRoom;
	private int roomWidth = 24, roomHeight = 12, exitWidth = 2, exitLength = 2;
	protected List<RoomTile> tiles = new List<RoomTile>();
	public Vector2Int position;
	public List<RoomObject> roomObjects = new List<RoomObject>();

	public delegate void ExitUnlockedEventHandler(Direction direction);
	public event ExitUnlockedEventHandler OnExitUnlocked;

	public Room(RoomType type, Vector2Int position)
	{
		this.type = type;
		this.position = position;
		GenerateContent();
	}

	protected virtual void GenerateContent()
	{
		for (int x = -1; x <= roomWidth; x++)
		{
			for (int y = -1; y <= roomHeight; y++)
			{
				RoomTile.TileType tileType = RoomTile.TileType.Floor;
				if (x == -1)
				{
					tileType = RoomTile.TileType.LeftWall;
					if (y == -1)
					{
						tileType = RoomTile.TileType.DownLeftInnerWall;
					}
					if (y == roomHeight)
					{
						tileType = RoomTile.TileType.UpLeftInnerWall;
					}
				}
				else if (x == roomWidth)
				{
					tileType = RoomTile.TileType.RightWall;
					if (y == -1)
					{
						tileType = RoomTile.TileType.DownRightInnerWall;
					}
					if (y == roomHeight)
					{
						tileType = RoomTile.TileType.UpRightInnerWall;
					}
				}
				else if (y == -1)
				{
					tileType = RoomTile.TileType.DownWall;
				}
				else if (y == roomHeight)
				{
					tileType = RoomTile.TileType.UpWall;
				}
				AddTile(x, y, tileType);
			}
		}
	}

	public void AddUpExit(int x)
	{
		if (upExit)
		{
			Debug.Log("Up exit already exists");
		}
		x = x < roomWidth - exitWidth - 1 ? x : roomWidth - exitWidth - 1;
		x = x >= 1 ? x : 1;
		AddExitFlooring(x, roomHeight, new Vector2Int(exitWidth, exitLength));
		for (int i = 0; i < exitLength; i++)
		{
			AddTile(x - 1, roomHeight + i,
				i == 0 ? RoomTile.TileType.DownRightOuterWall : RoomTile.TileType.LeftWall);
			AddTile(x + exitWidth, roomHeight + i,
				i == 0 ? RoomTile.TileType.DownLeftOuterWall : RoomTile.TileType.RightWall);
		}
		SetExit(Direction.Up, true, x, roomHeight);
		RoomExitTrigger newTrigger = new RoomExitTrigger(Direction.Up);
		newTrigger.position = new Vector2Int(x, roomHeight);
		roomObjects.Add(newTrigger);
	}

	public void AddRightExit(int y)
	{
		if (rightExit)
		{
			Debug.Log("Right exit already exists");
		}
		y = y < roomHeight - exitWidth - 1 ? y : roomHeight - exitWidth - 1;
		y = y >= 1 ? y : 1;
		AddExitFlooring(roomWidth, y, new Vector2Int(exitLength, exitWidth));
		for (int i = 0; i < exitLength; i++)
		{
			AddTile(roomWidth + i, y + exitWidth,
				i == 0 ? RoomTile.TileType.DownLeftOuterWall : RoomTile.TileType.UpWall);
			AddTile(roomWidth + i, y - 1,
				i == 0 ? RoomTile.TileType.UpLeftOuterWall : RoomTile.TileType.DownWall);
		}
		SetExit(Direction.Right, true, roomWidth, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(Direction.Right);
		newTrigger.position = new Vector2Int(roomWidth, y);
		roomObjects.Add(newTrigger);
	}

	public void AddDownExit(int x)
	{
		if (downExit)
		{
			Debug.Log("Down exit already exists");
		}
		x = x < roomWidth - exitWidth - 1 ? x : roomWidth - exitWidth - 1;
		x = x >= 1 ? x : 1;
		AddExitFlooring(x, -exitLength, new Vector2Int(exitWidth, exitLength));
		for (int i = 0; i < exitLength; i++)
		{
			AddTile(x - 1, -1 - i,
				i == 0 ? RoomTile.TileType.UpRightOuterWall : RoomTile.TileType.LeftWall);
			AddTile(x + exitWidth, -1 - i,
				 i == 0 ? RoomTile.TileType.UpLeftOuterWall : RoomTile.TileType.RightWall);
		}
		SetExit(Direction.Down, true, x, -exitLength);
		RoomExitTrigger newTrigger = new RoomExitTrigger(Direction.Down);
		newTrigger.position = new Vector2Int(x, -1);
		roomObjects.Add(newTrigger);
	}

	public void AddLeftExit(int y)
	{
		if (leftExit)
		{
			Debug.Log("Left exit already exists");
		}
		y = y < roomHeight - exitWidth - 1 ? y : roomHeight - exitWidth - 1;
		y = y >= 1 ? y : 1;
		AddExitFlooring(-exitLength, y, new Vector2Int(exitLength, exitWidth));
		for (int i = 0; i < exitLength; i++)
		{
			AddTile(-1 - i, y + exitWidth,
				i == 0 ? RoomTile.TileType.DownRightOuterWall : RoomTile.TileType.UpWall);
			AddTile(-1 - i, y - 1,
				i == 0 ? RoomTile.TileType.UpRightOuterWall : RoomTile.TileType.DownWall);
		}
		SetExit(Direction.Left, true, -exitLength, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(Direction.Left);
		newTrigger.position = new Vector2Int(-1, y);
		roomObjects.Add(newTrigger);
	}

	private void AddExitFlooring(int x, int y, Vector2Int dimensions)
	{
		for (int i = 0; i < dimensions.x; i++)
		{
			for (int j = 0; j < dimensions.y; j++)
			{
				AddTile(x + i, y + j, RoomTile.TileType.Floor);
			}
		}
	}

	protected void AddTile(int x, int y, RoomTile.TileType type)
	{
		int tileIndex = TileIndex(x, y);
		if (tileIndex != -1)
		{
			tiles[tileIndex] = new RoomTile(x, y, type);
		}
		else
		{
			tiles.Add(new RoomTile(x, y, type));
		}
	}

	protected int TileIndex(int x, int y)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].position.x == x && tiles[i].position.y == y) return i;
		}
		return -1;
	}

	public List<RoomTile> GetTiles() => tiles;

	public void SetWidth(int width) => roomWidth = width;

	public void SetHeight(int height) => roomHeight = height;

	public void SetDimensions(int width, int height)
	{
		SetWidth(width);
		SetHeight(height);
	}

	public void SetDimensions(Vector2Int dimensions)
	{
		roomWidth = dimensions.x;
		roomHeight = dimensions.y;
	}

	public int GetWidth() => roomWidth;

	public int GetHeight() => roomHeight;

	public Vector2Int GetDimensions()
		=> new Vector2Int(roomWidth + exitLength * 2, roomHeight + exitLength * 2);

	public Vector2 GetCenter() => new Vector2(GetWidth() / 2f, GetHeight() / 2f);

	public void SetRoom(Room room, Direction direction)
	{
		switch (direction)
		{
			case Direction.Up:
				upRoom = room;
				break;
			case Direction.Right:
				rightRoom = room;
				break;
			case Direction.Down:
				downRoom = room;
				break;
			case Direction.Left:
				leftRoom = room;
				break;
		}
	}

	public Room GetRoom(Direction direction)
	{
		switch (direction)
		{
			default: return null;
			case Direction.Up:
				return upRoom;
			case Direction.Right:
				return rightRoom;
			case Direction.Down:
				return downRoom;
			case Direction.Left:
				return leftRoom;
		}
	}

	public void Lock(Direction direction, RoomKey.KeyColour colour)
	{
		RoomLock newLock = CreateLock(direction, colour);
		roomObjects.Add(newLock);

		switch (direction)
		{
			case Direction.Up:
				upLock = newLock;
				upLocked = true;
				break;
			case Direction.Right:
				rightLock = newLock;
				rightLocked = true;
				break;
			case Direction.Down:
				downLock = newLock;
				downLocked = true;
				break;
			case Direction.Left:
				leftLock = newLock;
				leftLocked = true;
				break;
		}
	}

	private RoomLock CreateLock(Direction direction, RoomKey.KeyColour colour)
	{
		RoomLock newLock = new RoomLock(this, colour, direction);
		newLock.position = GetExitPos(direction);
		return newLock;
	}

	public Vector2Int GetExitPos(Direction direction)
	{
		switch (direction)
		{
			default: return Vector2Int.zero;
			case Direction.Up:
				return upExitPos;
			case Direction.Right:
				return rightExitPos;
			case Direction.Down:
				return downExitPos;
			case Direction.Left:
				return leftExitPos;
		}
	}

	public void AddKey(RoomKey.KeyColour colour)
	{
		RoomKey key = new RoomKey(colour);
		key.position = new Vector2Int(
			Random.Range(0, roomWidth), Random.Range(0, roomHeight));
		roomObjects.Add(key);
	}

	public bool AttemptUnlock(Direction direction)
	{
		if (!HasExit(direction)) return false;
		if (!IsLocked(direction)) return true;

		RoomLock directionLock = GetLock(direction);
		directionLock.Unlock();
		roomObjects.Remove(directionLock);
		Room otherRoom = GetRoom(direction);
		RoomLock otherRoomLock = otherRoom.GetLock(Opposite(direction));
		otherRoomLock.Unlock();
		otherRoom.roomObjects.Remove(otherRoomLock);

		OnExitUnlocked?.Invoke(direction);
		return true;
	}

	public void Unlock(Direction direction)
	{
		switch (direction)
		{
			case Direction.Up:
				upLocked = false;
				upLock = null;
				break;
			case Direction.Right:
				rightLocked = false;
				rightLock = null;
				break;
			case Direction.Down:
				downLocked = false;
				downLock = null;
				break;
			case Direction.Left:
				leftLocked = false;
				leftLock = null;
				break;
		}
	}

	public void RemoveObject(RoomObject obj) => roomObjects.Remove(obj);

	public static Direction Opposite(Direction direction)
	{
		switch (direction)
		{
			default: return direction;
			case Direction.Up:
				return Direction.Down;
			case Direction.Right:
				return Direction.Left;
			case Direction.Down:
				return Direction.Up;
			case Direction.Left:
				return Direction.Right;
		}
	}

	private RoomLock GetLock(Direction direction)
	{
		switch (direction)
		{
			default: return null;
			case Direction.Up:
				return upLock;
			case Direction.Right:
				return rightLock;
			case Direction.Down:
				return downLock;
			case Direction.Left:
				return leftLock;
		}
	}

	public bool IsLocked(Direction direction)
	{
		switch (direction)
		{
			default: return false;
			case Direction.Up:
				return upLocked;
			case Direction.Right:
				return rightLocked;
			case Direction.Down:
				return downLocked;
			case Direction.Left:
				return leftLocked;
		}
	}

	public bool HasExit(Direction direction)
	{
		switch (direction)
		{
			default: return false;
			case Direction.Up:
				return upExit;
			case Direction.Right:
				return rightExit;
			case Direction.Down:
				return downExit;
			case Direction.Left:
				return leftExit;
		}
	}

	public void SetExit(Direction direction, bool set, int x, int y)
	{
		switch (direction)
		{
			case Direction.Up:
				upExit = set;
				upExitPos = new Vector2Int(x, y);
				break;
			case Direction.Right:
				rightExit = set;
				rightExitPos = new Vector2Int(x, y);
				break;
			case Direction.Down:
				downExit = set;
				downExitPos = new Vector2Int(x, y);
				break;
			case Direction.Left:
				leftExit = set;
				leftExitPos = new Vector2Int(x, y);
				break;
		}
	}

	public int ExitCount()
	{
		int count = 0;
		if (upExit) count++;
		if (rightExit) count++;
		if (downExit) count++;
		if (leftExit) count++;
		return count;
	}
}