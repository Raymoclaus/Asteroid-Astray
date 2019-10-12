using UnityEngine;

namespace MovementBehaviours
{
	public class RangedChasingBehaviour : FollowBehaviour
	{
		[SerializeField] private float stoppingRange = 4f;
		[SerializeField] private bool alwaysFaceTarget;

		public override void TriggerUpdate()
		{
			base.TriggerUpdate();

			if (ShouldTurnToFaceTarget)
			{
				FaceDirection(TargetDirection);
			}
		}

		protected override bool ShouldChase => base.ShouldChase && !IsWithinStoppingRange;

		protected virtual bool IsWithinStoppingRange => DistanceToTarget <= stoppingRange;

		protected virtual bool ShouldTurnToFaceTarget => alwaysFaceTarget && TargetIsNearby;
	}

}