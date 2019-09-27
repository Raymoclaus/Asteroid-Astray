using System.Collections.Generic;
using UnityEngine;
using MazePuzzle;
using TileLightsPuzzle;
using BlockPushPuzzle;

public class Room
{
	public RoomType type;
	private bool upExit, rightExit, downExit, leftExit;
	private Vector2Int upExitPos, rightExitPos, downExitPos, leftExitPos;
	private bool upLocked, rightLocked, downLocked, leftLocked;
	private RoomLock upLock, rightLock, downLock, leftLock;
	private Room upRoom, rightRoom, downRoom, leftRoom;
	protected int roomWidth = 28, roomHeight = 16, exitWidth = 1, exitLength = 3;
	protected List<RoomTile> tiles = new List<RoomTile>();
	public Vector2Int position;
	public List<RoomObject> roomObjects = new List<RoomObject>();
	private PuzzleTypeWeightings puzzleWeightings;
	public Room previousRoom;

	public delegate void ExitUnlockedEventHandler(Direction direction);
	public event ExitUnlockedEventHandler OnExitUnlocked;

	public delegate void ChangeExitPositionEventHandler(Direction direction, Vector2Int pos);
	public event ChangeExitPositionEventHandler OnChangeExitPosition;

	public delegate void MazeAddedEventHandler(Maze maze);
	public event MazeAddedEventHandler OnMazeAdded;
	public delegate void TileLightsAddedEventHandler(TileGrid tileGrid);
	public event TileLightsAddedEventHandler OnTileLightsAdded;
	public delegate void BlockPushAddedEventHandler(PushPuzzle puzzle);
	public event BlockPushAddedEventHandler OnBlockPushAdded;

	public Room(RoomType type, Vector2Int position,
		PuzzleTypeWeightings puzzleWeightings, Room previousRoom)
	{
		this.type = type;
		this.position = position;
		this.puzzleWeightings = puzzleWeightings;
		this.previousRoom = previousRoom;
	}

	public void GenerateContent()
	{
		GenerateEmptyFloor();

		switch (type)
		{
			case RoomType.Start:
				GenerateStartRoom();
				break;
			case RoomType.Empty:
				break;
			case RoomType.Puzzle:
				GeneratePuzzleRoom();
				break;
			case RoomType.Enemies:
				GenerateEnemiesRoom();
				break;
			case RoomType.Treasure:
				break;
			case RoomType.Boss:
				break;
			case RoomType.NPC:
				break;
		}
	}

	private void GenerateEmptyFloor()
	{
		for (int x = 1; x < GetWidth() - 1; x++)
		{
			for (int y = 1; y < GetHeight() - 1; y++)
			{
				AddTile(x, y, RoomTile.TileType.Floor);
			}
		}
	}

	public void GenerateOuterWalls()
	{
		for (int x = 1 - exitLength; x < roomWidth - 1 + exitLength; x++)
		{
			for (int y = 1 - exitLength; y < roomHeight - 1 + exitLength; y++)
			{
				if (x <= 0
					|| x >= roomWidth - 1
					|| y <= 0
					|| y >= roomHeight - 1) AddTile(x, y, RoomTile.TileType.Wall);
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
				AddTile(x - 1, roomHeight - 1 + i, RoomTile.TileType.Wall);
				AddTile(x + exitWidth, roomHeight - 1 + i, RoomTile.TileType.Wall);
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
				AddTile(roomWidth - 1 + i, y + exitWidth, RoomTile.TileType.Wall);
				AddTile(roomWidth - 1 + i, y - 1, RoomTile.TileType.Wall);
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
				AddTile(x - 1, -i, RoomTile.TileType.Wall);
				AddTile(x + exitWidth, -i, RoomTile.TileType.Wall);
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
				AddTile(-i, y + exitWidth, RoomTile.TileType.Wall);
				AddTile(-i, y - 1, RoomTile.TileType.Wall);
			}
		}
	}

	private void GenerateStartRoom()
	{
		RoomLandingPad landingPad = new RoomLandingPad();
		landingPad.SetPosition(new Vector2Int(GetWidth() / 2, GetHeight() / 2 - 3));
		roomObjects.Add(landingPad);
	}

	private void GeneratePuzzleRoom()
	{
		PuzzleType pType = PickRandomPuzzleType();

		switch (pType)
		{
			case PuzzleType.Maze:
				GenerateMazePuzzle();
				break;
			case PuzzleType.TileLights:
				GenerateTileLightsPuzzle();
				break;
			case PuzzleType.BeamRedirection:
				break;
			case PuzzleType.BlockPush:
				GenerateBlockPushPuzzle();
				break;
			case PuzzleType.PatternMatching:
				break;
		}
	}

	private void GenerateMazePuzzle()
	{
		MazeGenerator gen = new MazeGenerator();
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

		Maze maze = gen.GeneratePuzzle(roomSize, exits);

		//exclude walls
		for (int x = 1; x < maze.GetSize().x - 1; x++)
		{
			for (int y = 1; y < maze.GetSize().y - 1; y++)
			{
				int index = maze.Index(x, y);
				bool wall = maze.Get(index);
				AddTile(x, y, wall ? RoomTile.TileType.Wall : RoomTile.TileType.Floor);
			}
		}

		OnMazeAdded?.Invoke(maze);
	}

	private void GenerateTileLightsPuzzle()
	{
		Vector2Int puzzleSize = new Vector2Int(GetWidth() / 2, GetHeight() / 2);
		int difficulty = 10;

		TileLightsGenerator gen = new TileLightsGenerator();
		TileGrid puzzleGrid = gen.GeneratePuzzle(puzzleSize, difficulty);

		Vector2Int offset = new Vector2Int(GetWidth() / 2 - puzzleGrid.GridSize.x / 2,
			GetHeight() / 2 - puzzleGrid.GridSize.y / 2);
		for (int i = 0; i < puzzleGrid.GetArrayLength(); i++)
		{
			Vector2Int position = puzzleGrid.GetPosition(i);

			bool flipped = puzzleGrid.IsFlipped(i);
			RoomTileLight tileLight = new RoomTileLight(puzzleGrid, i);
			tileLight.SetPosition(position + offset);
			roomObjects.Add(tileLight);
		}

		OnTileLightsAdded?.Invoke(puzzleGrid);
	}

	private void GenerateBlockPushPuzzle()
	{
		BlockPushGenerator gen = new BlockPushGenerator();
		Vector2Int puzzleSize = new Vector2Int(GetWidth() - 2, GetHeight() - 2);
		int padding = 1;
		int minimumSolutionCount = 1;
		PushPuzzle puzzle = gen.Generate(puzzleSize, padding, minimumSolutionCount);

		Vector2Int offset = Vector2Int.one;

		for (int i = 0; i < puzzle.grid.Length; i++)
		{
			Vector2Int position = puzzle.GetPositionFromIndex(i);
			bool isBlock = puzzle.BlockExists(position);
			if (!isBlock) continue;

			RoomPushableBlock roomBlock = new RoomPushableBlock(puzzle, position);
			roomBlock.SetPosition(position + offset);
			roomObjects.Add(roomBlock);
		}
		RoomGreenGroundButton finishButton = new RoomGreenGroundButton();
		finishButton.SetPosition(puzzle.finishTile + offset);
		finishButton.OnButtonTriggered += puzzle.CompletePuzzle;
		roomObjects.Add(finishButton);

		for (int i = 0; i < puzzle.resetTiles.Length; i++)
		{
			RoomRedGroundButton resetButton = new RoomRedGroundButton();
			resetButton.SetPosition(puzzle.resetTiles[i] + offset);
			resetButton.SubscribeToHeldEvent(puzzle.RevertLastChange, 1f / 10f);
			roomObjects.Add(resetButton);
		}

		OnBlockPushAdded?.Invoke(puzzle);
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
			return PuzzleType.TileLights;
		}
		if ((randomValue = randomValue - ptw.randomBeamRedirectionRoomWeighting) < 0f)
		{
			return PuzzleType.BeamRedirection;
		}
		return PuzzleType.BlockPush;
	}

	private void GenerateEnemiesRoom()
	{
		List<RoomEnemy> enemies = EnemyRoomData.GenerateChallenge(2f);
		roomObjects.AddRange(enemies);

		for (int i = 0; i < enemies.Count; i++)
		{
			int xPos = Random.Range(3, GetWidth() - 3);
			int yPos = Random.Range(3, GetHeight() - 3);
			enemies[i].SetPosition(new Vector2Int(xPos, yPos));
		}
	}

	public void AddUpExit(int x)
	{
		int y = roomHeight - 1;
		SetExit(Direction.Up, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Up);
		newTrigger.SetPosition(new Vector2Int(x, y));
		roomObjects.Add(newTrigger);
	}

	public void AddRightExit(int y)
	{
		int x = roomWidth - 1;
		SetExit(Direction.Right, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Right);
		newTrigger.SetPosition(new Vector2Int(x, y));
		roomObjects.Add(newTrigger);
	}

	public void AddDownExit(int x)
	{
		int y = 0;
		SetExit(Direction.Down, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Down);
		newTrigger.SetPosition(new Vector2Int(x, y));
		roomObjects.Add(newTrigger);
	}

	public void AddLeftExit(int y)
	{
		int x = 0;
		SetExit(Direction.Left, true, x, y);
		RoomExitTrigger newTrigger = new RoomExitTrigger(this, Direction.Left);
		newTrigger.SetPosition(new Vector2Int(x, y));
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

	public int GetWidth() => roomWidth;

	public int GetHeight() => roomHeight;

	public Vector2Int GetInnerDimensions() => new Vector2Int(GetWidth(), GetHeight());

	public Vector3 GetWorldSpacePosition()
	{
		Vector2Int worldPosition = position * GetInnerDimensions()
			+ position * (exitLength - 1) * 2;
		return new Vector2(worldPosition.x, worldPosition.y);
	}

	public Vector2Int GetDimensions()
		=> new Vector2Int(roomWidth + (exitLength - 1) * 2, roomHeight + (exitLength - 1) * 2);

	public Vector2 GetCenter() => new Vector2(GetWidth() / 2f, GetHeight() / 2f);
	public Vector2Int GetCenterInt()
	{
		Vector2 floatCenter = GetCenter();
		return new Vector2Int((int)floatCenter.x, (int)floatCenter.y);
	}

	public Bounds GetRoomSpaceBounds()
		=> new Bounds(GetCenter(), new Vector3(GetWidth(), GetHeight()));

	public Bounds GetWorldSpaceBounds()
		=> new Bounds(GetWorldSpacePosition(), new Vector3(GetWidth(), GetHeight()));

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
				break;
			case Direction.Right:
				rightLock = newLock;
				break;
			case Direction.Down:
				downLock = newLock;
				break;
			case Direction.Left:
				leftLock = newLock;
				break;
		}
		LockWithoutKey(direction);
	}

	public void LockWithoutKey(Direction direction)
	{
		switch (direction)
		{
			case Direction.Up:
				upLocked = true;
				break;
			case Direction.Right:
				rightLocked = true;
				break;
			case Direction.Down:
				downLocked = true;
				break;
			case Direction.Left:
				leftLocked = true;
				break;
		}
	}

	private RoomLock CreateLock(Direction direction, RoomKey.KeyColour colour)
	{
		RoomLock newLock = new RoomLock(this, colour, direction);
		newLock.SetPosition(GetExitPos(direction));
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
		RoomKey key = new RoomKey(this, colour);
		Vector2Int pos = new Vector2Int(
			Random.Range(1, roomWidth - 1), Random.Range(1, roomHeight - 1));
		key.SetPosition(pos);
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

		OnExitUnlocked?.Invoke(direction);
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

	public RoomExitTrigger GetExit(Direction direction)
	{
		for (int i = 0; i < roomObjects.Count; i++)
		{
			if (roomObjects[i].GetObjectType() == RoomObject.ObjType.ExitTrigger)
			{
				RoomExitTrigger exitTrigger = (RoomExitTrigger)roomObjects[i];
				if (exitTrigger.direction == direction) return exitTrigger;
			}
		}
		return null;
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

	public bool HasWallAtPosition(Vector3 roomSpacePosition)
	{
		Vector2Int intPosition = new Vector2Int(
			(int)roomSpacePosition.x, (int)roomSpacePosition.y);
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].position == intPosition
				&& tiles[i].type == RoomTile.TileType.Wall) return true;
		}
		return false;
	}

	public Vector3 WorldToRoomSpace(Vector3 worldPosition)
		=> worldPosition - GetWorldSpacePosition();
}