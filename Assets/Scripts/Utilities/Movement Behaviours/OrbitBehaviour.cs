using UnityEngine;

public class OrbitBehaviour : CriticalDistanceBehaviour
{
	[SerializeField] private float orbitRange = 5f;
	[SerializeField] private float orbitSpeed = 0.7f;
	[SerializeField] private float movementSmoothingPower = 0.5f;

	protected virtual float GetIntendedAngle() => Time.time * orbitSpeed;

	protected Vector3 OrbitPosition()
	{
		float angle = GetIntendedAngle();
		return TargetPosition + new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)) * orbitRange;
	}

	protected override Vector3 GoalLocation => OrbitPosition();

	protected override bool IsWithinStoppingRange => false;

	protected override float MovementSmoothingPower => movementSmoothingPower;
}
