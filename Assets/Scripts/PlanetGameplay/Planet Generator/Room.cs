using System.Collections.Generic;
using UnityEngine;
using MazeGenerator;

public class Room
{
	public RoomType type;
	private bool upExit, rightExit, downExit, leftExit;
	private Vector2Int upExitPos, rightExitPos, downExitPos, leftExitPos;
	private bool upLocked, rightLocked, downLocked, leftLocked;
	private RoomLock upLock, rightLock, downLock, leftLock;
	private Room upRoom, rightRoom, downRoom, leftRoom;
	protected int roomWidth = 28, roomHeight = 16, exitWidth = 1, exitLength = 2;
	protected List<RoomTile> tiles = new List<RoomTile>();
	public Vector2Int position;
	public List<RoomObject> roomObjects = new List<RoomObject>();
	private PuzzleTypeWeightings puzzleWeightings;

	public delegate void ExitUnlockedEventHandler(Direction direction);
	public event ExitUnlockedEventHandler OnExitUnlocked;

	public delegate void ChangeExitPositionEventHandler(Direction direction, Vector2Int pos);
	public event ChangeExitPositionEventHandler OnChangeExitPosition;

	public Room(RoomType type, Vector2Int position, PuzzleTypeWeightings puzzleWeightings)
	{
		this.type = type;
		this.position = position;
		this.puzzleWeightings = puzzleWeightings;
	}

	public void GenerateContent()
	{
		switch (type)
		{
			case RoomType.Start:
				GenerateStartRoom();
				break;
			case RoomType.Empty:
				GenerateEmptyRoom();
				break;
			case RoomType.Puzzle:
				GeneratePuzzleRoom();
				break;
			case RoomType.Enemies:
				GenerateEmptyRoom();
				break;
			case RoomType.Treasure:
				GenerateEmptyRoom();
				break;
			case RoomType.Boss:
				GenerateEmptyRoom();
				break;
			case RoomType.NPC:
				GenerateEmptyRoom();
				break;
		}
	}

	public void GenerateOuterWalls()
	{
		for (int x = 0; x < roomWidth; x++)
		{
			for (int y = 0; y < roomHeight; y++)
			{
				RoomTile.TileType tileType = RoomTile.TileType.Floor;
				if (x == 0)
				{
					tileType = RoomTile.TileType.LeftWall;
					if (y == 0)
					{
						tileType = RoomTile.TileType.DownLeftInnerWall;
					}
					if (y == roomHeight - 1)
					{
						tileType = RoomTile.TileType.UpLeftInnerWall;
					}
					AddTile(x, y, tileType);
				}
				else if (x == roomWidth - 1)
				{
					tileType = RoomTile.TileType.RightWall;
					if (y == 0)
					{
						tileType = RoomTile.TileType.DownRightInnerWall;
					}
					if (y == roomHeight - 1)
					{
						tileType = RoomTile.TileType.UpRightInnerWall;
					}
					AddTile(x, y, tileType);
				}
				else if (y == 0)
				{
					tileType = RoomTile.TileType.DownWall;
					AddTile(x, y, tileType);
				}
				else if (y == roomHeight - 1)
				{
					tileType = RoomTile.TileType.UpWall;
					AddTile(x, y, tileType);
				}
			}
		}

		if (upExit)
		{
			int x = upExitPos.x;
			x = x < roomWidth - exitWidth - 1 ? x : roomWidth - exitWidth - 1;
			x = x >= 1 ? x : 1;
			AddExitFlooring(x, roomHeight - 1, new Vector2Int(exitWidth, exitLength));
			for (int i = 0; i < exitLength; i++)
			{
				AddTile(x - 1, roomHeight - 1 + i,
					i == 0 ? RoomTile.TileType.DownRightOuterWall : RoomTile.TileType.LeftWall);
				AddTile(x + exitWidth, roomHeight - 1 + i,
					i == 0 ? RoomTile.TileType.DownLeftOuterWall : RoomTile.TileType.RightWall);
			}
		}
		if (rightExit)
		{
			int y = rightExitPos.y;
			y = y < roomHeight - exitWidth - 1 ? y : roomHeight - exitWidth - 1;
			y = y >= 1 ? y : 1;
			AddExitFlooring(roomWidth - 1, y, new Vector2Int(exitLength, exitWidth));
			for (int i = 0; i < exitLength; i++)
			{
				AddTile(roomWidth - 1 + i, y + exitWidth,
					i == 0 ? RoomTile.TileType.DownLeftOuterWall : RoomTile.TileType.UpWall);
				AddTile(roomWidth - 1 + i, y - 1,
					i == 0 ? RoomTile.TileType.UpLeftOuterWall : RoomTile.TileType.DownWall);
			}
		}
		if (downExit)
		{
			int x = downExitPos.x;
			x = x < roomWidth - exitWidth - 1 ? x : roomWidth - exitWidth - 1;
			x = x >= 1 ? x : 1;
			AddExitFlooring(x, 1 - exitLength, new Vector2Int(exitWidth, exitLength));
			for (int i = 0; i < exitLength; i++)
			{
				AddTile(x - 1, -i,
					i == 0 ? RoomTile.TileType.UpRightOuterWall : RoomTile.TileType.LeftWall);
				AddTile(x + exitWidth, -i,
					 i == 0 ? RoomTile.TileType.UpLeftOuterWall : RoomTile.TileType.RightWall);
			}
		}
		if (leftExit)
		{
			int y = leftExitPos.y;
			y = y < roomHeight - exitWidth - 1 ? y : roomHeight - exitWidth - 1;
			y = y >= 1 ? y : 1;
			AddExitFlooring(1 - exitLength, y, new Vector2Int(exitLength, exitWidth));
			for (int i = 0; i < exitLength; i++)
			{
				AddTile(-i, y + exitWidth,
					i == 0 ? RoomTile.TileType.DownRightOuterWall : RoomTile.TileType.UpWall);
				AddTile(-i, y - 1,
					i == 0 ? RoomTile.TileType.UpRightOuterWall : RoomTile.TileType.DownWall);
			}
		}
	}

	private void GenerateEmptyRoom()
	{
		for (int x = 1; x < roomWidth - 1; x++)
		{
			for (int y = 1; y < roomHeight - 1; y++)
			{
				RoomTile.TileType tileType = RoomTile.TileType.Floor;
				AddTile(x, y, tileType);
			}
		}
	}

	private void GenerateStartRoom()
	{
		GenerateEmptyRoom();
		PlanetLandingPad landingPad = new PlanetLandingPad();
		landingPad.position = new Vector2Int(GetWidth() / 2, GetHeight() / 2 - 3);
		roomObjects.Add(landingPad);
	}

	private void GeneratePuzzleRoom()
	{
		PuzzleType pType = PickRandomPuzzleType();
		GenerateEmptyRoom();

		switch (pType)
		{
			case PuzzleType.Maze:
				GenerateMazePuzzle();
				break;
			case PuzzleType.TileLight:
				break;
			case PuzzleType.BeamRedirection:
				break;
			case PuzzleType.BlockPush:
				break;
			case PuzzleType.PatternMatching:
				break;
		}
	}

	private void GenerateMazePuzzle()
	{
		Generator gen = new Generator();
		Vector2Int roomSize = new Vector2Int(GetWidth(), GetHeight());
		Vector2Int[] exits = new Vector2Int[ExitCount()];

		int count = 0;
		if (upExit)
		{
			//Debug.Log("Up: " + upExitPos);
			exits[count] = new Vector2Int(upExitPos.x, roomSize.y - 2);
			Debug.Log("Up: " + exits[count]);
			count++;
		}

		if (rightExit)
		{
			//Debug.Log("Right: " + rightExitPos);
			exits[count] = new Vector2Int(roomSize.x - 2, rightExitPos.y);
			Debug.Log("Right: " + exits[count]);
			count++;
		}

		if (downExit)
		{
			//Debug.Log("Down: " + downExitPos);
			exits[count] = new Vector2Int(downExitPos.x, 1);
			Debug.Log("Down: " + exits[count]);
			count++;
		}
		if (leftExit)
		{
			//Debug.Log("Left: " + leftExitPos);
			exits[count] = new Vector2Int(1, leftExitPos.y);
			Debug.Log("Left: " + exits[count]);
			count++;
		}

		Maze maze = gen.Generate(roomSize, exits);

		//exclude walls
		for (int x = 1; x < maze.GetSize().x - 1; x++)
		{
			for (int y = 1; y < maze.GetSize().y - 1; y++)
			{
				int index = maze.Index(x, y);
				bool wall = maze.Get(index);
				AddTile(x, y, wall ? RoomTile.TileType.UpWall : RoomTile.TileType.Floor);
			}
		}

		//reset exits
		//for (int i = 0; i < exits.Length; i++)
		//{
		//	Debug.Log("A: " + exits[i]);
		//	//left exit
		//	if (exits[i].x == 1)
		//	{
		//		SetExit(Direction.Left, true,
		//			0, exits[i].y);
		//		leftRoom.SetExit(Direction.Right, true,
		//			leftRoom.rightExitPos.x, exits[i].y);
		//	}
		//	//right exit
		//	if (exits[i].x == roomSize.x - 2)
		//	{
		//		SetExit(Direction.Right, true,
		//			roomSize.x - 1, exits[i].y);
		//		rightRoom.SetExit(Direction.Left, true,
		//			rightRoom.leftExitPos.x, exits[i].y);
		//	}
		//	//down exit
		//	if (exits[i].y == 1)
		//	{
		//		SetExit(Direction.Down, true,
		//			exits[i].x, exits[i].y);
		//		downRoom.SetExit(Direction.Up, true,
		//			exits[i].x, downRoom.upExitPos.y);
		//	}
		//	//up exit
		//	if (exits[i].y == roomSize.y - 2)
		//	{
		//		SetExit(Direction.Up, true,
		//			exits[i].x, exits[i].y);
		//		upRoom.SetExit(Direction.Down, true,
		//			exits[i].x, upRoom.downExitPos.y);
		//	}
		//}
	}

	private PuzzleType PickRandomPuzzleType()
	{
		PuzzleTypeWeightings ptw = puzzleWeightings;
		if (ptw == null) return PuzzleType.Maze;

		float totalWeighting =
			ptw.randomMazeRoomWeighting +
			ptw.randomTileLightRoomWeighting +
			ptw.randomBeamRedirectionRoomWeighting +
			ptw.randomBlockPushRoomWeighting;
		float randomValue = Random.Range(0f, totalWeighting);

		if ((randomValue = randomValue - ptw.randomMazeRoomWeighting) < 0f)
		{
			return PuzzleType.Maze;
		}
		if ((randomValue = randomValue - ptw.randomTileLightRoomWeighting) < 0f)
		{
			return PuzzleType.TileLight;
		}
		if ((randomValue = randomValue - ptw.randomBeamRedirectionRoomWeighting) < 0f)
		{
			return PuzzleType.BeamRedirection;
		}
		return PuzzleType.BlockPush;
	}

	public void AddUpExit(int x)
	{
		int y = roomHeight - 1;
		SetExit(Direction.Up, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Up);
		newTrigger.position = new Vector2Int(x, y);
		roomObjects.Add(newTrigger);
	}

	public void AddRightExit(int y)
	{
		int x = roomWidth - 1;
		SetExit(Direction.Right, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Right);
		newTrigger.position = new Vector2Int(x, y);
		roomObjects.Add(newTrigger);
	}

	public void AddDownExit(int x)
	{
		int y = 0;
		SetExit(Direction.Down, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Down);
		newTrigger.position = new Vector2Int(x, y);
		roomObjects.Add(newTrigger);
	}

	public void AddLeftExit(int y)
	{
		int x = 0;
		SetExit(Direction.Left, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Left);
		newTrigger.position = new Vector2Int(x, y);
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
		=> new Vector2Int(roomWidth + (exitLength - 1) * 2, roomHeight + (exitLength - 1) * 2);

	public Vector2 GetCenter() => new Vector2(GetWidth() / 2f - 0.5f, GetHeight() / 2f - 0.5f);

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
		OnChangeExitPosition?.Invoke(direction, new Vector2Int(x, y));
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