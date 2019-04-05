using UnityEngine;

public class DirectionMarker : MonoBehaviour
{
	[SerializeField] private float radius = 0.5f;
	private Transform parent;
	private Vector2 locationTarget;
	[SerializeField] private ShuttleTrackers shuttleTrackerSO;

	private void Awake()
	{
		parent = transform.parent;
		shuttleTrackerSO.NavigationUpdated += UpdateHUD;
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		gameObject?.SetActive(shuttleTrackerSO.navigationActive);
	}

	private void Update()
	{
		//get angle of current position to target position in degrees
		float angle = GetAngle();
		//rotate transform by angle
		transform.eulerAngles = Vector3.back * angle;
		//place transform at the current position relative to the parent
		angle *= Mathf.Deg2Rad;
		transform.position = parent.position + new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * radius;
	}

	private float GetAngle()
	{
		return -Vector2.SignedAngle(Vector2.up, shuttleTrackerSO.GetWaypointLocation() - parent.position);
	}
}