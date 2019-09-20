using UnityEngine;

public class PlanetRoomExitTrigger : PlanetNonSolid
{
	private static RoomViewer viewer;
	private static RoomViewer Viewer => viewer ?? (viewer = FindObjectOfType<RoomViewer>());

	private static PlanetGenerator generator;
	private static PlanetGenerator Generator
		=> generator ?? (generator = FindObjectOfType<PlanetGenerator>());
	
	[HideInInspector] public Direction direction;
	[SerializeField] private GameObject invisibleWallTilePrefab;
	private RoomExitTrigger exitTrigger;

	public void GoNext() => Generator.Go(direction);
	public Vector2Int DirectionValue
	{
		get
		{
			switch (direction)
			{
				default: return Vector2Int.up;
				case Direction.Up: return Vector2Int.up;
				case Direction.Right: return Vector2Int.right;
				case Direction.Down: return Vector2Int.down;
				case Direction.Left: return Vector2Int.left;
			}
		}
	}

	public override void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(room, roomObject, dataSet);

		exitTrigger = (RoomExitTrigger)roomObject;
		direction = exitTrigger.direction;
		if (room.IsLocked(direction))
		{
			EnableTrigger(false);
		}

		room.OnExitUnlocked += CheckUnlocked;
		GameObject invisibleWallTile = Instantiate(invisibleWallTilePrefab);
		Vector2Int dirVal = DirectionValue;
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

	private void Exit()
	{
		Generator.Go(direction);
	}
}