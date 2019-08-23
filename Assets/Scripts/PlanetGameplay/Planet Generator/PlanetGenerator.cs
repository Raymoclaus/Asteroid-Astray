using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomViewer))]
public class PlanetGenerator : MonoBehaviour
{
	private PlanetData planetData;
	private Room startRoom;
	private RoomViewer viewer;
	private Room activeRoom;

	public delegate void RoomChangedEventHandler(Room newRoom, Direction direction);
	public static event RoomChangedEventHandler OnRoomChanged;

	private void Start()
	{
		viewer = GetComponent<RoomViewer>();
		planetData = Generate();
		activeRoom = startRoom;
		//LoadCurrentRoom();
		StartCoroutine(viewer.ShowAllRooms(planetData));
	}

	private PlanetData Generate()
	{
		//return TestGeneration();

		PlanetData data = new PlanetData();
		
		Room currentRoom = null;
		int keyLevel = 0;

		//rule 1 -	Create starter room
		startRoom = data.AddRoom(RoomType.Start, new Vector2Int(0, 0));

		//rule 2 -	"Current Room" is starter room
		currentRoom = startRoom;

		//rule 7 -	Repeat steps 3 - 6 Y amount of times.
		int branchCount = 4;
		for (int j = 0; j < branchCount; j++)
		{
			//rule 3 -	Create a single branch from the "Current room" until X rooms have been
			//			created. Branch can't overlap with existing rooms. If branch meets a dead
			//			end, end the branch.
			int branchLength = 3;
			for (int i = 0; i < branchLength; i++)
			{
				currentRoom = CreateRandomExit(data, currentRoom);
			}

			//rule 4 -	Place a key at the end of that branch
			currentRoom.AddKey((RoomKey.KeyColour)keyLevel);

			//rule 5 -	Create a locked exit to a new room from any existing room except the end of
			//			that branch.
			List<Room> existingRooms = data.GetRooms();
			Room lockRoom = currentRoom;
			do
			{
				int randomIndex = UnityEngine.Random.Range(0, existingRooms.Count);
				lockRoom = existingRooms[randomIndex];
			} while (lockRoom == currentRoom || lockRoom.ExitCount() == 4);
			lockRoom = CreateRandomExit(data, lockRoom, true, (RoomKey.KeyColour)keyLevel);
			keyLevel++;

			//rule 6 -	"Current room" is the new room on the other side of the locked exit
			currentRoom = lockRoom;
		}

		//rule 8 -	"Current room" is the final boss room
		currentRoom.type = RoomType.Boss;

		return data;
	}

	private PlanetData TestGeneration()
	{
		PlanetData data = new PlanetData();

		startRoom = data.AddRoom(RoomType.Start, 0, 0);
		Room endRoom = data.AddRoom(RoomType.Treasure, -1, 1);
		Room aRoom = data.AddRoom(RoomType.Empty, -1, 0);
		Room bRoom = data.AddRoom(RoomType.Empty, 0, 1);

		aRoom.AddKey(RoomKey.KeyColour.Blue);
		bRoom.AddKey(RoomKey.KeyColour.Red);

		ConnectHorizontally(aRoom, startRoom);
		LockVertically(startRoom, bRoom, RoomKey.KeyColour.Blue);
		LockVertically(aRoom, endRoom, RoomKey.KeyColour.Red);

		return data;
	}

	private Room CreateRandomExit(PlanetData data, Room room, bool locked = false,
		RoomKey.KeyColour colour = RoomKey.KeyColour.Blue)
	{
		if (room.ExitCount() == 4) return null;

		List<Direction> directions = new List<Direction>
			{
				Direction.Up, Direction.Right, Direction.Down, Direction.Left
			};
		//remove any directions that are not available
		for (int j = directions.Count - 1; j >= 0; j--)
		{
			Vector2Int roomPos = AddDirection(room.position, directions[j]);
			if (data.GetRoomAtPosition(roomPos) != null)
			{
				directions.RemoveAt(j);
			}
		}

		//if no available directions then we're in a dead end
		if (directions.Count == 0) return null;

		Direction randomDirection = directions[UnityEngine.Random.Range(0, directions.Count)];
		Vector2Int pos = AddDirection(room.position, randomDirection);
		Room newRoom = data.AddRoom(RoomType.Empty, pos);
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
		int exitXPos = UnityEngine.Random.Range(1, lower.GetWidth() - 1);
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
		int exitYPos = UnityEngine.Random.Range(1, left.GetHeight() - 1);
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

	private Vector2Int AddDirection(Vector2Int v, Direction dir)
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

	public bool Go(Direction direction)
	{
		activeRoom = activeRoom.GetRoom(direction) ?? activeRoom;
		LoadCurrentRoom();
		OnRoomChanged?.Invoke(activeRoom, direction);
		return true;
	}

	private void LoadCurrentRoom() => viewer.ShowRoom(planetData, activeRoom, Vector2.zero);

	private Room GetRoomAtPosition(int x, int y) => GetRoomAtPosition(new Vector2Int(x, y));

	private Room GetRoomAtPosition(Vector2Int position) => planetData.GetRoomAtPosition(position);

	private bool RoomExists(Vector2Int position) => GetRoomAtPosition(position) != null;
}
