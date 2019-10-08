using UnityEngine;

public class PlanetRoomExitTrigger : PlanetNonSolid
{
	
	private Direction direction;
	[SerializeField] private GameObject invisibleWallTilePrefab;
	private RoomExitTrigger exitTrigger;
	
	public IntPair DirectionValue
	{
		get
		{
			switch (direction)
			{
				default: return IntPair.up;
				case Direction.Up: return IntPair.up;
				case Direction.Right: return IntPair.right;
				case Direction.Down: return IntPair.down;
				case Direction.Left: return IntPair.left;
			}
		}
	}

	public override void Setup(RoomViewer roomViewer, Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(roomViewer, room, roomObject, dataSet);

		exitTrigger = (RoomExitTrigger)roomObject;
		direction = exitTrigger.direction;
		if (room.IsLocked(direction))
		{
			EnableTrigger(false);
		}

		room.OnExitUnlocked += CheckUnlocked;
		GameObject invisibleWallTile = Instantiate(invisibleWallTilePrefab);
		IntPair dirVal = DirectionValue;
		invisibleWallTile.transform.position =
			transform.position + new Vector3(dirVal.x, dirVal.y, 0f);
		invisibleWallTile.transform.parent = transform;
	}

	private void OnDisable() => room.OnExitUnlocked -= CheckUnlocked;

	private void CheckUnlocked(Direction dir)
	{
		if (dir != direction) return;
		EnableTrigger(true);
	}

	protected override void Interacted(Triggerer actor)
	{
		base.Interacted(actor);
		Exit();
	}

	private void Exit() => roomViewer.Go(direction);
}