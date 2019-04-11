using UnityEngine;

public class Move : MonoBehaviour
{
	public float speed;
	public Vector3 direction;

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
}
