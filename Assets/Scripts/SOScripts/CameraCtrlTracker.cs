using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/CameraCtrlTracker")]
public class CameraCtrlTracker : ScriptableObject
{
	public float minCamSize = 1.7f;
	[HideInInspector]
	public float camSize;
	[HideInInspector]
	public ChunkCoords coords;
}
