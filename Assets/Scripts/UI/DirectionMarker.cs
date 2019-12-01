using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DirectionMarker : MonoBehaviour
{
	[SerializeField] private float radius = 0.5f;
	private Shuttle mainChar;
	private Shuttle MainChar => mainChar ?? (mainChar = FindObjectOfType<Shuttle>());
	private Vector2 LocationTarget => Vector2.zero;
	private Transform Parent => MainChar?.transform ?? transform.parent;
	private SpriteRenderer sprRend;
	private SpriteRenderer SprRend
		=> sprRend != null ? sprRend : (sprRend = GetComponent<SpriteRenderer>());

	public void Activate(bool active)
	{
		if (SprRend == null) return;
		SprRend.enabled = active;
	}

	private void Update()
	{
		if (!SprRend.enabled) return;
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
		return Parent?.position ?? transform.position;
	}
}