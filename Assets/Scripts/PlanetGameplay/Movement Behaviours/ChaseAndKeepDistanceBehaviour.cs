using UnityEngine;

public class ChaseAndKeepDistanceBehaviour : RangedChasingBehaviour
{
	[SerializeField] private float runAwayRange = 3f;

	protected override void Update()
	{
		base.Update();
		if (IsWithinRunAwayRange())
		{
			PhysicsController.MoveAwayFromPosition(GetTargetPosition());
		}
	}

	protected override bool ShouldTurnToFaceTarget()
		=> base.ShouldTurnToFaceTarget() && !IsWithinRunAwayRange();

	public bool IsWithinRunAwayRange() => DistanceToTarget() <= runAwayRange;
}
