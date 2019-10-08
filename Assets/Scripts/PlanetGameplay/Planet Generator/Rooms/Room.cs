using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Room : ITextSaveLoader
{
	private PlanetData planetData;
	private DirectionalArray<bool> exitExists = new DirectionalArray<bool>();
	private DirectionalArray<IntPair> exitPositions = new DirectionalArray<IntPair>();
	private DirectionalArray<bool> exitLocked = new DirectionalArray<bool>();
	private DirectionalArray<List<RoomLock>> exitLocks = new DirectionalArray<List<RoomLock>>();
	private DirectionalArray<Room> neighbourRooms = new DirectionalArray<Room>();
	private DirectionalArray<int> boundaries = new DirectionalArray<int>();
	private const int ROOM_WIDTH = 28, ROOM_HEIGHT = 16, EXIT_WIDTH = 2, EXIT_LENGTH = 3;
	protected List<RoomTile> tiles = new List<RoomTile>();
	public IntPair position;
	public List<RoomObject> roomObjects = new List<RoomObject>();
	public Room previousRoom;
	public IntPair previousRoomPosition;

	public delegate void ExitUnlockedEventHandler(Direction direction);
	public event ExitUnlockedEventHandler OnExitUnlocked;

	public delegate void ChangeExitPositionEventHandler(Direction direction, IntPair pos);
	public event ChangeExitPositionEventHandler OnChangeExitPosition;

	public Room()
	{
		boundaries.Left = 1;
		boundaries.Down = 1;
		boundaries.Up = RoomHeight - 2;
		boundaries.Right = RoomWidth - 2;

		for (int i = 0; i < exitLocks.Length; i++)
		{
			Direction dir = (Direction)i;
			exitLocks[dir] = new List<RoomLock>();
		}
	}

	public Room(string[] lines, PlanetData data) : this()
	{
		planetData = data;
		Load(lines);
	}

	public Room(IntPair position, Room previousRoom) : this()
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
				AddTile(tilePos, RoomTile.TileType.Floor);
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
					AddTile(tilePos, RoomTile.TileType.Wall);
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
					RoomTile.TileType tileType =
						j == -1 || j == EXIT_WIDTH
						? RoomTile.TileType.Wall
						: RoomTile.TileType.Floor;
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
			RoomExitTrigger newTrigger = new RoomExitTrigger(this, direction);
			IntPair lineIntDir = direction.Clockwise().ToPositiveDirection().ToVector2();
			newTrigger.SetPosition(exitPos + lineIntDir * i);
			roomObjects.Add(newTrigger);
		}
	}

	public void SetNeighbourRoom(Room neighbour, Direction direction)
		=> neighbourRooms[direction] = neighbour;

	protected void AddTileLine(IntPair startPos, RoomTile.TileType type, int length, Direction direction)
	{
		IntPair intDir = direction.ToVector2();
		for (int i = 0; i < length; i++)
		{
			IntPair tilePos = startPos + intDir * i;
			AddTile(tilePos, type);
		}
	}

	protected void AddTile(IntPair tilePos, RoomTile.TileType type)
	{
		int tileIndex = TileIndex(tilePos);
		if (tileIndex != -1)
		{
			tiles[tileIndex] = new RoomTile(this, tilePos, type);
		}
		else
		{
			tiles.Add(new RoomTile(this, tilePos, type));
		}
	}

	protected int TileIndex(IntPair tilePos)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].Position.x == tilePos.x
				&& tiles[i].Position.y == tilePos.y) return i;
		}
		return -1;
	}

	public List<RoomTile> Tiles => tiles;

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

	public Room GetRoom(Direction direction) => neighbourRooms[direction];

	public IntPair GetRoomPositionInDirection(Direction direction)
		=> position + IntPair.GetDirection(direction);

	public void Lock(Direction direction, RoomKey.KeyColour colour)
	{
		for (int i = 0; i < EXIT_WIDTH; i++)
		{
			RoomLock newLock = CreateLock(direction, colour, i);
			roomObjects.Add(newLock);
			exitLocks[direction].Add(newLock);
		}
		LockWithoutKey(direction);
	}

	public void LockWithoutKey(Direction direction) => exitLocked[direction] = true;

	protected void LockAllExceptPreviousRoom()
	{
		Direction[] directions = (Direction[])System.Enum.GetValues(typeof(Direction));
		for (int i = 0; i < directions.Length; i++)
		{
			if (GetRoom(directions[i]) != previousRoom) LockWithoutKey(directions[i]);
		}
	}

	protected void UnlockAllExits()
	{
		Direction[] directions = (Direction[])System.Enum.GetValues(typeof(Direction));
		for (int i = 0; i < directions.Length; i++)
		{
			Unlock(directions[i]);
		}
	}

	private RoomLock CreateLock(Direction direction, RoomKey.KeyColour colour, int index)
	{
		ItemStack stack = new ItemStack(RoomKey.ConvertToItemType(colour), 1);
		RoomLock newLock = new RoomLock(this, colour, direction, stack);
		IntPair lineIntDir = direction.Clockwise().ToPositiveDirection().ToVector2();
		newLock.SetPosition(GetExitPos(direction) + lineIntDir * index);
		return newLock;
	}

	public IntPair GetExitPos(Direction direction) => exitPositions[direction];

	public void AddKey(RoomKey.KeyColour colour)
	{
		RoomKey key = new RoomKey(this, colour);
		IntPair pos = new IntPair(
			Random.Range(1, RoomWidth - 1), Random.Range(1, RoomHeight - 1));
		key.SetPosition(pos);
		roomObjects.Add(key);
	}

	public void Unlock(Direction direction)
	{
		RemoveLocks(direction);

		Room neighbourRoom = GetRoom(direction);
		Direction oppositeDirection = direction.Opposite();
		neighbourRoom.RemoveLocks(oppositeDirection);
	}

	private void RemoveLocks(Direction direction)
	{
		exitLocked[direction] = false;
		List<RoomLock> locks = exitLocks[direction];
		for (int i = 0; i < exitLocks[direction].Count; i++)
		{
			RoomLock directionLock = locks[i];
			roomObjects.Remove(directionLock);
		}
		locks.Clear();
		OnExitUnlocked?.Invoke(direction);
	}

	public void RemoveObject(RoomObject obj) => roomObjects.Remove(obj);

	public bool IsLocked(Direction direction) => exitLocked[direction];

	public List<RoomObject> GetObjects<T>() where T : RoomObject
		=> roomObjects.Where(t => t.GetType().IsAssignableFrom(typeof(T))).ToList();

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
			&& t.type == RoomTile.TileType.Wall);

	public Vector3 WorldToRoomSpace(Vector3 worldPosition)
		=> worldPosition - WorldSpacePosition;

	public void PrepareForSaving() { }

	public virtual void FinishedLoading()
	{
		for (int i = 0; i < Direction.Up.EnumEntryCount(); i++)
		{
			Direction dir = (Direction)i;
			IntPair neighbourRoomPosition = GetRoomPositionInDirection(dir);
			neighbourRooms[dir] = planetData.GetRoomAtPosition(neighbourRoomPosition);
		}
		previousRoom = planetData.GetRoomAtPosition(previousRoomPosition);
	}

	public ITextSaveLoader[] GetObjectsToSave() => tiles.Concat(roomObjects).ToArray();

	public const string SAVE_TAG = "[Room]", SAVE_END_TAG = "[/Room]";
	public virtual string Tag => SAVE_TAG;

	public virtual string EndTag => SAVE_END_TAG;

	public virtual string GetSaveText(int indentLevel)
		=> $"{new string('\t', indentLevel)}{upExitProp}:{exitExists[Direction.Up]}\n" +
		$"{new string('\t', indentLevel)}{rightExitProp}:{exitExists[Direction.Right]}\n" +
		$"{new string('\t', indentLevel)}{downExitProp}:{exitExists[Direction.Down]}\n" +
		$"{new string('\t', indentLevel)}{leftExitProp}:{exitExists[Direction.Left]}\n" +
		$"{new string('\t', indentLevel)}{upExitPosProp}:{exitPositions[Direction.Up]}\n" +
		$"{new string('\t', indentLevel)}{rightExitPosProp}:{exitPositions[Direction.Right]}\n" +
		$"{new string('\t', indentLevel)}{downExitPosProp}:{exitPositions[Direction.Down]}\n" +
		$"{new string('\t', indentLevel)}{leftExitPosProp}:{exitPositions[Direction.Left]}\n" +
		$"{new string('\t', indentLevel)}{upLockedProp}:{exitLocked[Direction.Up]}\n" +
		$"{new string('\t', indentLevel)}{rightLockedProp}:{exitLocked[Direction.Right]}\n" +
		$"{new string('\t', indentLevel)}{downLockedProp}:{exitLocked[Direction.Down]}\n" +
		$"{new string('\t', indentLevel)}{leftLockedProp}:{exitLocked[Direction.Left]}\n" +
		$"{new string('\t', indentLevel)}{positionProp}:{position}\n" +
		$"{new string('\t', indentLevel)}{previousRoomPositionProp}:{(previousRoom != null ? previousRoom.position : position)}\n";
	
	private static readonly string upExitProp = "upExit";
	private static readonly string rightExitProp = "rightExit";
	private static readonly string downExitProp = "downExit";
	private static readonly string leftExitProp = "leftExit";
	private static readonly string upExitPosProp = "upExitPos";
	private static readonly string rightExitPosProp = "rightExitPos";
	private static readonly string downExitPosProp = "downExitPos";
	private static readonly string leftExitPosProp = "leftExitPos";
	private static readonly string upLockedProp = "upLocked";
	private static readonly string rightLockedProp = "rightLocked";
	private static readonly string downLockedProp = "downLocked";
	private static readonly string leftLockedProp = "leftLocked";
	private static readonly string positionProp = "position";
	private static readonly string previousRoomPositionProp = "previousRoomPosition";
	public virtual void Load(string[] lines)
	{
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			if (line == RoomTile.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomTile.SAVE_END_TAG);
				tiles.Add(new RoomTile(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomBoomee.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomBoomee.SAVE_END_TAG);
				roomObjects.Add(new RoomBoomee(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomCharger.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomCharger.SAVE_END_TAG);
				roomObjects.Add(new RoomCharger(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomDummy.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomDummy.SAVE_END_TAG);
				roomObjects.Add(new RoomDummy(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomExitTrigger.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomExitTrigger.SAVE_END_TAG);
				roomObjects.Add(new RoomExitTrigger(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomGargantula.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomGargantula.SAVE_END_TAG);
				roomObjects.Add(new RoomGargantula(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomGreenGroundButton.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomGreenGroundButton.SAVE_END_TAG);
				roomObjects.Add(new RoomGreenGroundButton(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomKey.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomKey.SAVE_END_TAG);
				roomObjects.Add(new RoomKey(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomLandingPad.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomLandingPad.SAVE_END_TAG);
				roomObjects.Add(new RoomLandingPad(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomLock.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomLock.SAVE_END_TAG);
				RoomLock newLock = new RoomLock(this, lines.SubArray(i, end));
				exitLocks[newLock.direction].Add(newLock);
				roomObjects.Add(newLock);
				i = end;
				continue;
			}

			if (line == RoomPlayer.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomPlayer.SAVE_END_TAG);
				roomObjects.Add(new RoomPlayer(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomRedGroundButton.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomRedGroundButton.SAVE_END_TAG);
				roomObjects.Add(new RoomRedGroundButton(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomSmokey.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomSmokey.SAVE_END_TAG);
				roomObjects.Add(new RoomSmokey(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomSpooder.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomSpooder.SAVE_END_TAG);
				roomObjects.Add(new RoomSpooder(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			if (line == RoomTreasureChest.SAVE_TAG)
			{
				int end = lines.IndexOfObjectAfterIndex(i, RoomTreasureChest.SAVE_END_TAG);
				roomObjects.Add(new RoomTreasureChest(this, lines.SubArray(i, end)));
				i = end;
				continue;
			}

			string[] props = line.Split(':');

			if (props[0] == upExitProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitExists[Direction.Up] = b;
				continue;
			}
			if (props[0] == rightExitProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitExists[Direction.Right] = b;
				continue;
			}
			if (props[0] == downExitProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitExists[Direction.Down] = b;
				continue;
			}
			if (props[0] == leftExitProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitExists[Direction.Left] = b;
				continue;
			}
			if (props[0] == upExitPosProp)
			{
				IntPair pos;
				IntPair.TryParse(props[1], out pos);
				exitPositions[Direction.Up] = pos;
				continue;
			}
			if (props[0] == rightExitPosProp)
			{
				IntPair pos;
				IntPair.TryParse(props[1], out pos);
				exitPositions[Direction.Right] = pos;
				continue;
			}
			if (props[0] == downExitPosProp)
			{
				IntPair pos;
				IntPair.TryParse(props[1], out pos);
				exitPositions[Direction.Down] = pos;
				continue;
			}
			if (props[0] == leftExitPosProp)
			{
				IntPair pos;
				IntPair.TryParse(props[1], out pos);
				exitPositions[Direction.Left] = pos;
				continue;
			}
			if (props[0] == upLockedProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitLocked[Direction.Up] = b;
				continue;
			}
			if (props[0] == rightLockedProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitLocked[Direction.Right] = b;
				continue;
			}
			if (props[0] == downLockedProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitLocked[Direction.Down] = b;
				continue;
			}
			if (props[0] == leftLockedProp)
			{
				bool b;
				bool.TryParse(props[1], out b);
				exitLocked[Direction.Left] = b;
				continue;
			}
			if (props[0] == positionProp)
			{
				IntPair.TryParse(props[1], out position);
				continue;
			}
			if (props[0] == previousRoomPositionProp)
			{
				IntPair.TryParse(props[1], out previousRoomPosition);
				continue;
			}
		}
	}
}