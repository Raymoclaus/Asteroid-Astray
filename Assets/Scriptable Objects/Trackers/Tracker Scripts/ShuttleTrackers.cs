using UnityEngine;

[CreateAssetMenu]
public class ShuttleTrackers : ScriptableObject
{
	[HideInInspector]
	public Vector3 position, rotation;
	[HideInInspector]
	public Vector2 velocity;
	[HideInInspector]
	public float lastLookDirection, boostRemaining;
	public float drillLaunchMaxAngle = 60f, drillLaunchSpeed = 10f, launchDamage = 500f;
	[HideInInspector]
	public int storageCount;
}
