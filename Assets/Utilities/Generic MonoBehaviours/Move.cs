using UnityEngine;

public class Move : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private Vector3 direction;

	private void Update()
	{
		Vector3 pos = CalculatePosition();
		SetPosition(pos);
	}

	private void SetPosition(Vector3 pos)
	{
		transform.position = pos;
	}

	private Vector3 CalculatePosition()
	{
		return transform.position + direction * speed * Time.deltaTime;
	}

	public float GetSpeed() => speed;

	public void SetSpeed(float speed) => this.speed = speed;

	public Vector3 GetDirection() => direction;

	public void SetXDirection(float x) => direction.x = x;

	public void SetYDirection(float y) => direction.y = y;

	public void SetZDirection(float z) => direction.z = z;

	public void SetDirection(Vector3 direction)
	{
		SetXDirection(direction.x);
		SetYDirection(direction.y);
		SetZDirection(direction.z);
	}
}
