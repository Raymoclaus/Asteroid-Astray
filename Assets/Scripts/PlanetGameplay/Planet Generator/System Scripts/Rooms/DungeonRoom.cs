using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CustomDataTypes;

public class DungeonRoom
{
	private PlanetData planetData;
	private DirectionalArray<bool> exitExists = new DirectionalArray<bool>();
	private DirectionalArray<IntPair> exitPositions = new DirectionalArray<IntPair>();
	private DirectionalArray<bool> exitLocked = new DirectionalArray<bool>();
	private DirectionalArray<int> exitLockType = new DirectionalArray<int>();
	private DirectionalArray<DungeonRoom> neighbourRooms = new DirectionalArray<DungeonRoom>();
	private DirectionalArray<int> boundaries = new DirectionalArray<int>();
	private const int ROOM_WIDTH = 38, ROOM_HEIGHT = 20, EXIT_WIDTH = 2, EXIT_LENGTH = 3;
	protected List<DungeonRoomTile> tiles = new List<DungeonRoomTile>();
	public IntPair position;
	public List<DungeonRoomObject> roomObjects = new List<DungeonRoomObject>();
	public DungeonRoom previousRoom;
	public IntPair previousRoomPosition;
	
	public event Action<Direction> OnExitUnlocked;
	public event Action<Direction, int> OnExitLocked;

	public delegate void ChangeExitPositionEventHandler(Direction direction, IntPair pos);
	public event ChangeExitPositionEventHandler OnChangeExitPosition;

	public DungeonRoom()
	{
		boundaries.Left = 1;
		boundaries.Down = 1;
		boundaries.Up = RoomHeight - 2;
		boundaries.Right = RoomWidth - 2;
	}

	public DungeonRoom(IntPair position, DungeonRoom previousRoom) : this()
	{
		this.position = new IntPair(position.x, position.y);
		this.previousRoom = previousRoom;
	}

	public virtual void GenerateContent() => GenerateEmptyFloor();

	private void GenerateEmptyFloor()
	{
		for (int x = 1; x < RoomWidth - 1; x++)
		{
			for (int y = 1; y < RoomHeight - 1; y++)
			{
				IntPair tilePos = new IntPair(x, y);
				AddTile(tilePos, DungeonRoomTileType.Floor);
			}
		}
	}

	public void GenerateOuterWalls()
	{
		for (int x = boundaries.Left - EXIT_LENGTH; x <= boundaries.Right + EXIT_LENGTH; x++)
		{
			for (int y = boundaries.Down - EXIT_LENGTH; y <= boundaries.Up + EXIT_LENGTH; y++)
			{
				if (x < boundaries.Left
					|| x > boundaries.Right
					|| y < boundaries.Down
					|| y > boundaries.Up)
				{
					IntPair tilePos = new IntPair(x, y);
					AddTile(tilePos, DungeonRoomTileType.Wall);
				}
			}
		}

		for (int i = 0; i < exitExists.Length; i++)
		{
			Direction dir = (Direction)i;
			if (exitExists[dir])
			{
				IntPair exitPos = exitPositions[dir];
				for (int j = -1; j < EXIT_WIDTH + 1; j++)
				{
					DungeonRoomTileType tileType =
						j == -1 || j == EXIT_WIDTH
						? DungeonRoomTileType.Wall
						: DungeonRoomTileType.Floor;
					Direction clockwiseDir = dir.Clockwise().ToPositiveDirection();
					IntPair lineIntDir = clockwiseDir.ToVector2();
					IntPair startPos = exitPos + lineIntDir * j;
					AddTileLine(startPos, tileType, EXIT_LENGTH, dir);
				}
			}
		}
	}

	public IntPair HorizontalBoundaries
		=> new IntPair(boundaries.Left, boundaries.Right);

	public IntPair VerticalBoundaries
		=> new IntPair(boundaries.Down, boundaries.Up);

	public int ExitWidth => EXIT_WIDTH;

	public void AddExit(Direction direction, IntPair exitPos)
	{
		bool isHorizontal = direction.IsHorizontal();
		IntPair intDirection = direction.ToVector2();
		if (isHorizontal)
		{
			exitPos.x = boundaries[direction] + intDirection.x;
		}
		else
		{
			exitPos.y = boundaries[direction] + intDirection.y;
		}
		SetExit(direction, true, exitPos);
		for (int i = 0; i < EXIT_WIDTH; i++)
		{
			IntPair lineIntDir = direction.Clockwise().ToPositiveDirection().ToVector2();
			IntPair pos = exitPos + lineIntDir * i;
			DungeonRoomObject newTrigger = new DungeonRoomObject(this, pos,
				"ExitTrigger", direction, false);
			roomObjects.Add(newTrigger);
		}
	}

	public void SetNeighbourRoom(DungeonRoom neighbour, Direction direction)
		=> neighbourRooms[direction] = neighbour;

	protected void AddTileLine(IntPair startPos, DungeonRoomTileType type, int length, Direction direction)
	{
		IntPair intDir = direction.ToVector2();
		for (int i = 0; i < length; i++)
		{
			IntPair tilePos = startPos + intDir * i;
			AddTile(tilePos, type);
		}
	}

	protected void AddTile(IntPair tilePos, DungeonRoomTileType type)
	{
		int tileIndex = TileIndex(tilePos);
		if (tileIndex != -1)
		{
			tiles[tileIndex] = new DungeonRoomTile(type, tilePos);
		}
		else
		{
			tiles.Add(new DungeonRoomTile(type, tilePos));
		}
	}

	protected int TileIndex(IntPair tilePos)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].Position == tilePos) return i;
		}
		return -1;
	}

	public List<DungeonRoomTile> Tiles => tiles;

	public int RoomWidth => ROOM_WIDTH;

	public int RoomHeight => ROOM_HEIGHT;

	public IntPair InnerDimensions => new IntPair(RoomWidth, RoomHeight);

	public Vector3 WorldSpacePosition => position * Dimensions;

	public IntPair Dimensions
		=> InnerDimensions + IntPair.one * (EXIT_LENGTH - 1) * 2;

	public Vector2 Center => new Vector2(RoomWidth / 2f, RoomHeight / 2f);

	public IntPair CenterInt => Center;

	public Bounds GetRoomSpaceBounds()
		=> new Bounds(Center, InnerDimensions);

	public Bounds GetWorldSpaceBounds()
		=> new Bounds(WorldSpacePosition, InnerDimensions);

	public DungeonRoom GetRoom(Direction direction) => neighbourRooms[direction];

	public IntPair GetRoomPositionInDirection(Direction direction)
		=> position + IntPair.GetDirection(direction);

	public void Lock(Direction direction, int lockID)
	{
		exitLocked[direction] = true;
		exitLockType[direction] = lockID;
		OnExitLocked?.Invoke(direction, lockID);
	}

	protected void LockAllExceptPreviousRoom()
	{
		Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));
		for (int i = 0; i < directions.Length; i++)
		{
			if (GetRoom(directions[i]) != previousRoom)
			{
				Lock(directions[i], 0);
			}
		}
	}

	protected void UnlockAllExitsOfLockType(int lockID)
	{
		Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));
		for (int i = 0; i < directions.Length; i++)
		{
			Direction direction = directions[i];
			if (exitLockType[direction] != lockID) continue;
			Unlock(direction);
		}
	}

	protected void UnlockAllExitsOfLockTypeNone()
		=> UnlockAllExitsOfLockType(0);

	public IntPair GetExitPos(Direction direction) => exitPositions[direction];

	public void AddKey(int lockID)
	{
		IntPair pos = GetRandomPositionInRoom;
		DungeonRoomObject key = new DungeonRoomObject(this, pos, $"Key{lockID}", null, false);
		roomObjects.Add(key);
	}

	public IntPair GetRandomPositionInRoom
		=> new IntPair(
			UnityEngine.Random.Range(HorizontalBoundaries.x, HorizontalBoundaries.y),
			UnityEngine.Random.Range(VerticalBoundaries.x, VerticalBoundaries.y));

	public void Unlock(Direction direction)
	{
		RemoveLock(direction);
		DungeonRoom neighbourRoom = GetRoom(direction);
		Direction oppositeDirection = direction.Opposite();
		neighbourRoom.RemoveLock(oppositeDirection);
	}

	private void RemoveLock(Direction direction)
	{
		exitLocked[direction] = false;
		OnExitUnlocked?.Invoke(direction);
	}

	public void RemoveObject(DungeonRoomObject obj) => roomObjects.Remove(obj);

	public bool IsLocked(Direction direction) => exitLocked[direction];

	public int LockID(Direction direction) => exitLockType[direction];

	public bool HasExit(Direction direction) => exitExists[direction];

	public void SetExit(Direction direction, bool set, IntPair pos)
	{
		exitExists[direction] = set;
		exitPositions[direction] = pos;
		OnChangeExitPosition?.Invoke(direction, pos);
	}

	public int ExitCount => exitExists.Count(t => t);

	public bool HasWallAtPosition(IntPair roomSpacePosition)
		=> tiles.Exists(
			t => t.Position == roomSpacePosition
			&& t.type == DungeonRoomTileType.Wall);

	public Vector3 WorldToRoomSpace(Vector3 worldPosition)
		=> worldPosition - WorldSpacePosition;
}