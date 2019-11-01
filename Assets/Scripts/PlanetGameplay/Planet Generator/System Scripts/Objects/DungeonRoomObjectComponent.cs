using UnityEngine;

public class DungeonRoomObjectComponent : MonoBehaviour
{
	public string objectName;
	public DungeonRoomViewer Viewer { get; private set; }
	public DungeonRoomObject RoomObject { get; private set; }

	public virtual void Setup(DungeonRoomViewer viewer, DungeonRoomObject roomObject)
	{
		Viewer = viewer;
		RoomObject = roomObject;
		Position = roomObject.Position;
	}

	public Vector3 Position
	{
		get => transform.position;
		set => transform.position = value;
	}

	public object Data => RoomObject.ObjectData;
}
