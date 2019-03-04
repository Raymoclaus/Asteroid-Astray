using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ShuttleTracker")]
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
	public bool autoPilot = false;
	public bool hasControl = true;
	public bool isKinematic = false;
	public bool isInvulnerable = false;
	public bool navigationActive = false;

	public void ToggleAutoPilot()
	{
		autoPilot = !autoPilot;
	}

	public void SetControllable(bool controllable)
	{
		hasControl = controllable;
	}

	public void SetKinematic(bool kinematic)
	{
		isKinematic = kinematic;
	}

	public void SetInvulnerable(bool invulnerable)
	{
		isInvulnerable = invulnerable;
	}
}
