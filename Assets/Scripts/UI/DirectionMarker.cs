using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DirectionMarker : MonoBehaviour
{
	[SerializeField] private float radius = 0.5f;
	private Shuttle mainChar;
	private Shuttle MainChar { get { return mainChar ?? (mainChar = FindObjectOfType<Shuttle>()); } }
	private Vector2 LocationTarget { get { return MainChar?.waypoint.GetPosition() ?? Vector2.zero; } }
	private Transform parent { get { return MainChar?.transform ?? transform.parent; } }
	private SpriteRenderer sprRend;

	private void Awake()
	{
		sprRend = GetComponent<SpriteRenderer>();
		MainChar.OnNavigationUpdated += Activate;
	}

	public void Activate(bool active) => sprRend.enabled = active;

	private void Update()
	{
		if (!sprRend.enabled) return;
		//get angle of current position to target position in degrees
		float angle = GetAngle();
		//rotate transform by angle
		transform.eulerAngles = Vector3.back * angle;
		//place transform at the current position relative to the parent
		angle *= Mathf.Deg2Rad;
		transform.position = MainChar.transform.position + new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * radius;
	}

	private float GetAngle()
	{
		return -Vector2.SignedAngle(Vector2.up, LocationTarget - GetCurrentPosition());
	}

	private Vector2 GetCurrentPosition()
	{
		return parent?.position ?? transform.position;
	}
}