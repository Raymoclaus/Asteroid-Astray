using UnityEngine;

public class PlanetRoomExitTrigger : PlanetNonSolid
{
	private static RoomViewer viewer;
	private static RoomViewer Viewer => viewer ?? (viewer = FindObjectOfType<RoomViewer>());

	private static PlanetGenerator generator;
	private static PlanetGenerator Generator
		=> generator ?? (generator = FindObjectOfType<PlanetGenerator>());
	
	public Direction direction = Direction.Up;

	public void GoNext() => Generator.Go(direction);

	public override void Setup(Room room, RoomObject roomObject, PlanetVisualData dataSet)
	{
		base.Setup(room, roomObject, dataSet);

		direction = ((RoomExitTrigger)roomObject).direction;
		if (room.IsLocked(direction))
		{
			EnableTrigger(false);
		}

		room.OnExitUnlocked += CheckUnlocked;
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