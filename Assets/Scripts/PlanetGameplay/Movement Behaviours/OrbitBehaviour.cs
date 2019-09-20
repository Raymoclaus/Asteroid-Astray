using UnityEngine;

public class OrbitBehaviour : ChaseAndKeepDistanceBehaviour
{
	[SerializeField] private float orbitRange = 5f;
	[SerializeField] private float orbitSpeed = 0.7f;

	protected virtual float GetIntendedAngle() => Time.time * orbitSpeed;

	protected Vector2 OrbitPosition()
	{
		float angle = GetIntendedAngle();
		return GetTargetPosition() + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * orbitRange;
	}

	protected override Vector2 GetGoalLocation() => OrbitPosition();

	protected override bool IsWithinStoppingRange() => false;

	protected override bool ShouldSlowDownWhenReachingGoalLocation() => true;
}
