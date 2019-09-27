using UnityEngine;

public class FollowBehaviour : TargetBasedBehaviour
{
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
	}

	protected virtual bool ShouldChase => true;

	protected virtual Vector3 GoalLocation => TargetPosition;
}
