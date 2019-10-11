using System;
using UnityEngine;

public class FollowBehaviour : TargetBasedBehaviour
{
	public float goalDistance = 0.1f;

	public event Action OnReachedTarget;

	public override void TriggerUpdate()
	{
		if (TargetIsNearby && ShouldChase)
		{
			MoveTowardsPosition(GoalLocation);
		}
		else
		{
			SlowDown();
		}

		if (IsWithinGoalDistance)
		{
			OnReachedTarget?.Invoke();
		}
	}

	protected bool IsWithinGoalDistance => DistanceToTarget > goalDistance;

	protected virtual bool ShouldChase => IsWithinGoalDistance;

	protected virtual Vector3 GoalLocation => TargetPosition;
}
