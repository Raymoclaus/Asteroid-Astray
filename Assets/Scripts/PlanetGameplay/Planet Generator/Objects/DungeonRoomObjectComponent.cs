using UnityEngine;

public class DungeonRoomObjectComponent : MonoBehaviour
{
	public string objectName;
	protected DungeonRoomViewer viewer;
	protected DungeonRoomObject roomObject;

	public virtual void Setup(DungeonRoomViewer viewer, DungeonRoomObject roomObject)
	{
		this.viewer = viewer;
		this.roomObject = roomObject;
		Position = roomObject.Position;
	}

	public Vector3 Position
	{
		get => transform.position;
		set => transform.position = value;
	}
}
