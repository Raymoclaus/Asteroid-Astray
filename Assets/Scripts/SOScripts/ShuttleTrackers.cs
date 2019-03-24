using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ShuttleTracker")]
public class ShuttleTrackers : ScriptableObject
{
	private Vector3 position;
	[HideInInspector] public Vector3 rotation;
	[HideInInspector] public Vector2 velocity;
	public bool canBoost = true, canDrill = true, canLaunch = true, canShoot = true;
	[HideInInspector] public float lastLookDirection, boostRemaining;
	public float drillLaunchMaxAngle = 60f, drillLaunchSpeed = 10f, launchDamage = 500f;
	[HideInInspector] public int storageCount;
	public bool autoPilot = false;
	public bool hasControl = true;
	public bool isKinematic = false;
	public bool isInvulnerable = false;
	public bool navigationActive = false;
	private Transform defaultWaypointTarget;
	private Transform waypointTarget;
	private Vector3? waypointLocation;

	private float timeLastMoved, timeLastGoInput;

	public void ResetDefaults()
	{
		autoPilot = false;
		timeLastMoved = 0f;
		timeLastGoInput = 0f;
	}

	public void SetPosition(Vector3 position)
	{
		if (this.position == position) return;
		this.position = position;
		timeLastMoved = Time.timeSinceLevelLoad;
	}

	public Vector3 GetPosition() => position;

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

	public delegate void NavigationUpdatedEventHandler();
	public event NavigationUpdatedEventHandler NavigationUpdated;
	public void SetNavigationActive(bool active)
	{
		if (active == navigationActive) return;
		navigationActive = active;
		NavigationUpdated?.Invoke();
	}

	public void SetDefaultWaypointTarget(Transform defaultTarget)
	{
		defaultWaypointTarget = defaultTarget;
	}

	public void SetWaypoint(Transform target, Vector3? waypoint)
	{
		this.waypointTarget = target;
		this.waypointLocation = waypoint;
	}

	public Vector3 GetWaypointLocation()
	{
		if (waypointTarget != null)
		{
			return waypointTarget.position;
		}
		if (waypointLocation != null)
		{
			return (Vector3)waypointLocation;
		}
		return defaultWaypointTarget.position;
	}

	public float GetDistanceToWaypoint()
	{
		Vector3 waypointLocation = GetWaypointLocation();
		float dist = Vector3.Distance(position, waypointLocation);
		if (dist < 2f) GameEvents.WaypointReached(waypointLocation);
		return dist;
	}

	public float GetTimeSinceMoved() => Time.timeSinceLevelLoad - timeLastMoved;

	public delegate void GoInputEventHandler();
	public event GoInputEventHandler OnGoInput;
	public void GoInput()
	{
		OnGoInput?.Invoke();
		timeLastGoInput = Time.timeSinceLevelLoad;
	}

	public delegate void LaunchInputEventHandler();
	public event LaunchInputEventHandler OnLaunchInput;
	public void LaunchInput() => OnLaunchInput?.Invoke();
}
