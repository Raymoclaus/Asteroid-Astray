using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/CameraCtrlTracker")]
public class CameraCtrlTracker : ScriptableObject
{
	public float minCamSize = 1.7f;
	[HideInInspector]
	public float camSize;
	[HideInInspector]
	public int RangeModifier { get { return (int)((camSize - minCamSize) / 5f); } }
	[HideInInspector]
	public ChunkCoords coords;
	public readonly int ENTITY_VIEW_RANGE = 1;
	[HideInInspector]
	public bool useConstantSize = false;
	[HideInInspector]
	public float constantSize = 2f;
	[HideInInspector]
	public bool useLookAheadModifier = false;
	[HideInInspector]
	public float distanceAheadModifier = 2f;
	[HideInInspector]
	public float moveAheadSpeedModifier = 2f;
	[HideInInspector]
	public Vector3 position;

	public bool IsCoordInView(ChunkCoords coord)
	{
		return ChunkCoords.MaxDistance(coord, coords) <= ENTITY_VIEW_RANGE + RangeModifier;
	}

	public bool IsCoordInPhysicsRange(ChunkCoords coord)
	{
		return ChunkCoords.MaxDistance(coord, coords) < Constants.MAX_PHYSICS_RANGE;
	}
}
