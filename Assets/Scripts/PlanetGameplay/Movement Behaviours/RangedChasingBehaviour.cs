using UnityEngine;

public class RangedChasingBehaviour : ChasingBehaviour
{
	[SerializeField] private float stoppingRange = 4f;
	[SerializeField] private bool alwaysFaceTarget;

	protected override void Update()
	{
		base.Update();

		if (ShouldTurnToFaceTarget())
		{
			PhysicsController.FaceDirection(target.position - PhysicsController.SelfPosition);
		}
	}

	protected override bool ShouldChase() => base.ShouldChase() && !IsWithinStoppingRange();

	protected virtual bool IsWithinStoppingRange() => DistanceToTarget() <= stoppingRange;

	protected virtual bool ShouldTurnToFaceTarget() => alwaysFaceTarget && TargetIsNearby();
}
