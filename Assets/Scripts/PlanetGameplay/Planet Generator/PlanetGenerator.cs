using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator
{
	public PlanetData Generate(float difficultySetting)
	{
		PlanetData data = new PlanetData();
		PlanetDifficultyModifiers difficultyModifiers
			= new PlanetDifficultyModifiers(difficultySetting);
		PlanetRoomTypeWeightings roomTypeWeightings = GetRoomTypeWeightings();

		Room currentRoom = null;
		int keyLevel = 0;

		//rule 1 -	Create starter room
		data.startRoom = data.AddRoom(RoomType.Start, new IntPair(0, 0), null,
			difficultyModifiers);

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
				Room newRoom = CreateRandomExit(data, currentRoom, false,
					RoomKey.KeyColour.Blue, false, roomTypeWeightings,
					difficultyModifiers);
				if (newRoom == currentRoom) break;
				currentRoom = newRoom;
			}

			//rule 4 -	Place a key at the end of that branch
			currentRoom.AddKey((RoomKey.KeyColour)keyLevel);

			//rule 5 -	Create a locked exit to a new room from any existing room except the end of
			//			that branch.
			List<Room> existingRooms = data.GetRooms();
			Room lockRoom = currentRoom;
			do
			{
				int randomIndex = Random.Range(0, existingRooms.Count);
				lockRoom = existingRooms[randomIndex];
			} while (lockRoom == currentRoom || lockRoom.ExitCount() == 4);
			lockRoom = CreateRandomExit(data, lockRoom, true,
				(RoomKey.KeyColour)keyLevel, j == branchCount - 1, roomTypeWeightings,
				difficultyModifiers);
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
			List<Room> existingRooms = data.GetRooms();
			Room deadEndStart = null;
			do
			{
				int randomIndex = Random.Range(0, existingRooms.Count);
				deadEndStart = existingRooms[randomIndex];
			} while (deadEndStart == data.finalRoom || deadEndStart.ExitCount() == 4);
			currentRoom = deadEndStart;

			int branchLength = Mathf.Max(1,
				Random.Range(difficultyModifiers.minBranchLength, difficultyModifiers.maxBranchLength));
			for (int j = 0; j < branchLength; j++)
			{
				Room newRoom = CreateRandomExit(data, currentRoom, false,
					RoomKey.KeyColour.Blue, false, roomTypeWeightings,
					difficultyModifiers);
				if (newRoom == currentRoom) break;
				currentRoom = newRoom;
				if (j == branchLength - 1)
				{
					newRoom.type = RoomType.Treasure;
				}
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

	private Room CreateRandomExit(PlanetData data, Room room, bool locked,
		RoomKey.KeyColour colour, bool bossRoom,
		PlanetRoomTypeWeightings roomTypeWeightings,
		PlanetDifficultyModifiers difficultyModifiers)
	{
		if (room.ExitCount() == 4) return room;

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
		RoomType type = bossRoom ? RoomType.Boss : PickRandomRoomType(roomTypeWeightings);
		Room newRoom = data.AddRoom(type, pos, room,
			difficultyModifiers);
		if (locked)
		{
			ConnectWithLock(room, newRoom, randomDirection, colour);
		}
		else
		{
			Connect(room, newRoom, randomDirection);
		}
		return newRoom;
	}

	private RoomType PickRandomRoomType(PlanetRoomTypeWeightings roomTypeWeightings)
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
			return RoomType.Empty;
		}
		if ((randomValue = randomValue - roomTypeWeightings.puzzleRoomWeighting) < 0f)
		{
			return RoomType.Puzzle;
		}
		if ((randomValue = randomValue - roomTypeWeightings.enemiesRoomWeighting) < 0f)
		{
			return RoomType.Enemies;
		}
		if ((randomValue = randomValue - roomTypeWeightings.treasureRoomWeighting) < 0f)
		{
			return RoomType.Treasure;
		}
		return RoomType.NPC;
	}

	private void Connect(Room a, Room b, Direction dir)
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

	private void ConnectWithLock(Room a, Room b, Direction dir, RoomKey.KeyColour colour)
	{
		switch (dir)
		{
			case Direction.Up:
				LockVertically(a, b, colour);
				break;
			case Direction.Right:
				LockHorizontally(a, b, colour);
				break;
			case Direction.Down:
				LockVertically(b, a, colour);
				break;
			case Direction.Left:
				LockHorizontally(b, a, colour);
				break;
		}
	}

	private void ConnectVertically(Room lower, Room upper)
	{
		int exitXPos = Random.Range(3, lower.GetWidth() - 3);
		lower.AddUpExit(exitXPos);
		lower.SetRoom(upper, Direction.Up);
		upper.AddDownExit(exitXPos);
		upper.SetRoom(lower, Direction.Down);
	}

	private void LockVertically(Room lower, Room upper, RoomKey.KeyColour lockColour)
	{
		ConnectVertically(lower, upper);
		lower.Lock(Direction.Up, lockColour);
		upper.Lock(Direction.Down, lockColour);
	}

	private void ConnectHorizontally(Room left, Room right)
	{
		int exitYPos = Random.Range(3, left.GetHeight() - 3);
		left.AddRightExit(exitYPos);
		left.SetRoom(right, Direction.Right);
		right.AddLeftExit(exitYPos);
		right.SetRoom(left, Direction.Left);
	}

	private void LockHorizontally(Room left, Room right, RoomKey.KeyColour lockColour)
	{
		ConnectHorizontally(left, right);
		left.Lock(Direction.Right, lockColour);
		right.Lock(Direction.Left, lockColour);
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

	private Room GetRoomAtPosition(PlanetData data, int x, int y) => GetRoomAtPosition(data, new IntPair(x, y));

	private Room GetRoomAtPosition(PlanetData data, IntPair position) => data.GetRoomAtPosition(position);

	private bool RoomExists(PlanetData data, IntPair position) => GetRoomAtPosition(data, position) != null;
}
