using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CustomDataTypes;

public class DungeonGenerator
{
	public PlanetData Generate(float difficultySetting)
	{
		PlanetData data = new PlanetData();
		PlanetDifficultyModifiers difficultyModifiers
			= new PlanetDifficultyModifiers(difficultySetting);
		PlanetRoomTypeWeightings roomTypeWeightings = GetRoomTypeWeightings();
		PuzzleTypeWeightings puzzleWeightings = GetPuzzleWeightings();

		DungeonRoom currentRoom = null;
		int keyLevel = 1;

		//rule 1 -	Create starter room
		DungeonRoom startRoom = new StartRoom(IntPair.zero, null);
		data.AddRoom(startRoom);
		data.startRoom = startRoom;

		//rule 2 -	"Current Room" is starter room
		currentRoom = data.startRoom;

		//rule 7 -	Repeat steps 3 - 6 Y amount of times.
		int branchCount = Mathf.Max(1, Random.Range(
			difficultyModifiers.minBranchCount,
			PlanetDifficultyModifiers.MAX_BRANCH_COUNT));
		for (int j = 0; j < branchCount; j++)
		{
			//rule 3 -	Create a single branch from the "Current room" until X rooms have been
			//			created. Branch can't overlap with existing rooms. If branch meets a dead
			//			end, end the branch.
			int branchLength = Mathf.Max(1,
				Random.Range(difficultyModifiers.minBranchLength, difficultyModifiers.maxBranchLength));
			for (int i = 0; i < branchLength; i++)
			{
				DungeonRoom newRoom = CreateRandomExit(data, currentRoom, false,
					keyLevel, false, false, roomTypeWeightings,
					puzzleWeightings, difficultyModifiers);
				if (newRoom == currentRoom) break;
				currentRoom = newRoom;
			}

			//rule 4 -	Place a key at the end of that branch
			currentRoom.AddKey(keyLevel);

			//rule 5 -	Create a locked exit to a new room from any existing room except the end of
			//			that branch.
			List<DungeonRoom> existingRooms = data.GetRooms();
			DungeonRoom lockRoom = currentRoom;
			do
			{
				int randomIndex = Random.Range(0, existingRooms.Count);
				lockRoom = existingRooms[randomIndex];
			} while (lockRoom == currentRoom || lockRoom.ExitCount == 4);
			lockRoom = CreateRandomExit(data, lockRoom, true,
				keyLevel, j == branchCount - 1, false,
				roomTypeWeightings, puzzleWeightings, difficultyModifiers);
			keyLevel++;

			//rule 6 -	"Current room" is the new room on the other side of the locked exit
			currentRoom = lockRoom;
		}

		//rule 8 -	"Current room" is the final room
		data.finalRoom = currentRoom;

		//rule 9 -	Create "Dead end" branches Z times of X length from any room except the boss
		//			room.
		branchCount = Mathf.Max(0, Random.Range(difficultyModifiers.minDeadEndCount, difficultyModifiers.maxDeadEndCount));
		for (int i = 0; i < branchCount; i++)
		{
			List<DungeonRoom> existingRooms = data.GetRooms();
			DungeonRoom deadEndStart = null;
			do
			{
				int randomIndex = Random.Range(0, existingRooms.Count);
				deadEndStart = existingRooms[randomIndex];
			} while (deadEndStart == data.finalRoom || deadEndStart.ExitCount == 4);
			currentRoom = deadEndStart;

			int branchLength = Mathf.Max(1,
				Random.Range(difficultyModifiers.minBranchLength, difficultyModifiers.maxBranchLength));
			for (int j = 0; j < branchLength; j++)
			{
				DungeonRoom newRoom = CreateRandomExit(data, currentRoom, false,
					keyLevel, false, j == branchLength - 1, roomTypeWeightings,
					puzzleWeightings, difficultyModifiers);
				if (newRoom == currentRoom) break;
				currentRoom = newRoom;
			}
		}

		for (int i = 0; i < data.GetRoomCount(); i++)
		{
			data.GetRooms()[i].GenerateContent();
		}

		for (int i = 0; i < data.GetRoomCount(); i++)
		{
			data.GetRooms()[i].GenerateOuterWalls();
		}

		return data;
	}

	private PuzzleTypeWeightings GetPuzzleWeightings()
		=> Resources.LoadAll<PuzzleTypeWeightings>(string.Empty).FirstOrDefault();

	private PlanetRoomTypeWeightings GetRoomTypeWeightings()
		=> Resources.LoadAll<PlanetRoomTypeWeightings>(string.Empty).FirstOrDefault();

	private DungeonRoom CreateRandomExit(PlanetData data, DungeonRoom room, bool locked,
		int lockID, bool bossRoom, bool treasure,
		PlanetRoomTypeWeightings roomTypeWeightings,
		PuzzleTypeWeightings puzzleWeightings,
		PlanetDifficultyModifiers difficultyModifiers)
	{
		if (room.ExitCount == 4) return room;

		List<Direction> directions = new List<Direction>
			{
				Direction.Up, Direction.Right, Direction.Down, Direction.Left
			};
		//remove any directions that are not available
		for (int j = directions.Count - 1; j >= 0; j--)
		{
			IntPair roomPos = AddDirection(room.position, directions[j]);
			if (data.GetRoomAtPosition(roomPos) != null)
			{
				directions.RemoveAt(j);
			}
		}

		//if no available directions then we're in a dead end
		if (directions.Count == 0) return room;

		Direction randomDirection = directions[Random.Range(0, directions.Count)];
		IntPair pos = AddDirection(room.position, randomDirection);

		DungeonRoom newRoom;
		if (bossRoom)
		{
			newRoom = new BossRoom(pos, room, difficultyModifiers.enemyRoomDifficulty);
		}
		else if (treasure)
		{
			newRoom = new TreasureRoom(pos, room);
		}
		else
		{
			newRoom = PickRandomRoom(pos, room, roomTypeWeightings,
			puzzleWeightings, difficultyModifiers);
		}
		data.AddRoom(newRoom);

		if (locked)
		{
			ConnectWithLock(room, newRoom, randomDirection, lockID);
		}
		else
		{
			Connect(room, newRoom, randomDirection);
		}

		return newRoom;
	}

	private DungeonRoom PickRandomRoom(IntPair position, DungeonRoom previousRoom,
		PlanetRoomTypeWeightings roomTypeWeightings,
		PuzzleTypeWeightings puzzleRoomWeightings,
		PlanetDifficultyModifiers difficultyModifiers)
	{
		float totalWeighting =
			roomTypeWeightings.emptyRoomWeighting +
			roomTypeWeightings.puzzleRoomWeighting +
			roomTypeWeightings.enemiesRoomWeighting +
			roomTypeWeightings.treasureRoomWeighting +
			roomTypeWeightings.npcRoomWeighting;
		float randomValue = Random.Range(0f, totalWeighting);

		if ((randomValue = randomValue - roomTypeWeightings.emptyRoomWeighting) < 0f)
		{
			return new DungeonRoom(position, previousRoom);
		}
		if ((randomValue = randomValue - roomTypeWeightings.puzzleRoomWeighting) < 0f)
		{
			return PickRandomPuzzleRoom(position, previousRoom, puzzleRoomWeightings);
		}
		if ((randomValue = randomValue - roomTypeWeightings.enemiesRoomWeighting) < 0f)
		{
			return new EnemyRoom(position, previousRoom,
				difficultyModifiers.enemyRoomDifficulty);
		}
		if ((randomValue = randomValue - roomTypeWeightings.treasureRoomWeighting) < 0f)
		{
			return new TreasureRoom(position, previousRoom);
		}
		return new NpcRoom(position, previousRoom);
	}

	private DungeonRoom PickRandomPuzzleRoom(IntPair position, DungeonRoom previousRoom,
		PuzzleTypeWeightings puzzleWeightings)
	{
		float totalWeighting =
			puzzleWeightings.randomMazeRoomWeighting +
			puzzleWeightings.randomTileLightRoomWeighting +
			puzzleWeightings.randomBeamRedirectionRoomWeighting +
			puzzleWeightings.randomBlockPushRoomWeighting +
			puzzleWeightings.randomPatternMatchRoomWeighting;
		float randomValue = Random.Range(0f, totalWeighting);

		if ((randomValue = randomValue - puzzleWeightings.randomMazeRoomWeighting) < 0f)
		{
			return new MazeRoom(position, previousRoom);
		}
		if ((randomValue = randomValue - puzzleWeightings.randomTileLightRoomWeighting) < 0f)
		{
			return new TileFlipPuzzleRoom(position, previousRoom);
		}
		if ((randomValue = randomValue - puzzleWeightings.randomBeamRedirectionRoomWeighting) < 0f)
		{
			return new DungeonRoom(position, previousRoom);
		}
		if ((randomValue = randomValue - puzzleWeightings.randomBlockPushRoomWeighting) < 0f)
		{
			return new BlockPushPuzzleRoom(position, previousRoom);
		}
		return new DungeonRoom(position, previousRoom);
	}

	private void Connect(DungeonRoom a, DungeonRoom b, Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				ConnectVertically(a, b);
				break;
			case Direction.Right:
				ConnectHorizontally(a, b);
				break;
			case Direction.Down:
				ConnectVertically(b, a);
				break;
			case Direction.Left:
				ConnectHorizontally(b, a);
				break;
		}
	}

	private void ConnectWithLock(DungeonRoom a, DungeonRoom b, Direction dir, int lockID)
	{
		switch (dir)
		{
			case Direction.Up:
				LockVertically(a, b, lockID);
				break;
			case Direction.Right:
				LockHorizontally(a, b, lockID);
				break;
			case Direction.Down:
				LockVertically(b, a, lockID);
				break;
			case Direction.Left:
				LockHorizontally(b, a, lockID);
				break;
		}
	}

	private void ConnectVertically(DungeonRoom lower, DungeonRoom upper)
	{
		IntPair boundaries = lower.HorizontalBoundaries;
		int exitWidth = lower.ExitWidth;
		boundaries.y -= exitWidth - 1;
		int randomPos = Random.Range(boundaries.x, boundaries.y);
		while (randomPos % exitWidth != 1)
		{
			randomPos--;
		}
		IntPair exitPos = new IntPair(randomPos, 0);
		lower.AddExit(Direction.Up, exitPos);
		upper.AddExit(Direction.Down, exitPos);
		lower.SetNeighbourRoom(upper, Direction.Up);
		upper.SetNeighbourRoom(lower, Direction.Down);
	}

	private void LockVertically(DungeonRoom lower, DungeonRoom upper, int lockID)
	{
		ConnectVertically(lower, upper);
		lower.Lock(Direction.Up, lockID);
		upper.Lock(Direction.Down, lockID);
	}

	private void ConnectHorizontally(DungeonRoom left, DungeonRoom right)
	{
		IntPair boundaries = left.VerticalBoundaries;
		int exitWidth = left.ExitWidth;
		boundaries.y -= exitWidth - 1;
		int randomPos = Random.Range(boundaries.x, boundaries.y);
		while (randomPos % exitWidth != 1)
		{
			randomPos--;
		}
		IntPair exitPos = new IntPair(0, randomPos);
		left.AddExit(Direction.Right, exitPos);
		right.AddExit(Direction.Left, exitPos);
		left.SetNeighbourRoom(right, Direction.Right);
		right.SetNeighbourRoom(left, Direction.Left);
	}

	private void LockHorizontally(DungeonRoom left, DungeonRoom right, int lockID)
	{
		ConnectHorizontally(left, right);
		left.Lock(Direction.Right, lockID);
		right.Lock(Direction.Left, lockID);
	}

	private IntPair AddDirection(IntPair v, Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				v.y++;
				break;
			case Direction.Right:
				v.x++;
				break;
			case Direction.Down:
				v.y--;
				break;
			case Direction.Left:
				v.x--;
				break;
		}
		return v;
	}

	private DungeonRoom GetRoomAtPosition(PlanetData data, int x, int y) => GetRoomAtPosition(data, new IntPair(x, y));

	private DungeonRoom GetRoomAtPosition(PlanetData data, IntPair position) => data.GetRoomAtPosition(position);

	private bool RoomExists(PlanetData data, IntPair position) => GetRoomAtPosition(data, position) != null;
}
