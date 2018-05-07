using UnityEngine;

public class Rotate : MonoBehaviour
{
	[Header("Rotations per second.")]
	public float xAxis, yAxis, zAxis = 1f;

	private void Update()
	{
		Vector3 rot = transform.eulerAngles;
		float delta = Time.deltaTime * 360f;
		rot.x += delta * xAxis;
		rot.y += delta * yAxis;
		rot.z += delta * zAxis;
		transform.eulerAngles = rot;
	}
}