using UnityEngine;

public class ChasingBehaviour : MovementBehaviour
{
	[SerializeField] protected Transform target;
	[SerializeField] private float chaseRange = 10f;

	protected virtual void Update()
	{
		if (TargetIsNearby() && ShouldChase())
		{
			PhysicsController.MoveTowardsPosition(GetGoalLocation(), ShouldSlowDownWhenReachingGoalLocation());
		}
		else
		{
			PhysicsController.SlowDown();
		}
	}

	public void SetTarget(Transform target) => this.target = target;

	protected virtual bool ShouldSlowDownWhenReachingGoalLocation() => false;

	protected virtual bool ShouldChase() => true;

	protected virtual bool TargetIsNearby()
	{
		if (target == null) return false;
		return DistanceToTarget() <= chaseRange;
	}

	protected virtual Vector2 GetGoalLocation() => GetTargetPosition();

	protected Vector2 GetTargetPosition() => target?.position ?? Vector2.zero;

	protected float DistanceToTarget() => Vector2.Distance(PhysicsController.SelfPosition, GetTargetPosition());
}
