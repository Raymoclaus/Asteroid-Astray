using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/BgCamTracker")]
public class BgCamTracker : ScriptableObject
{
	[Range(1f, 179f)]
	public float fieldOfView = 1f;
	[HideInInspector]
	public Vector3 position;
}
