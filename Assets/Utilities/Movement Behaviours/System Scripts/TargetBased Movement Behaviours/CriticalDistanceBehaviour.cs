using UnityEngine;

namespace MovementBehaviours
{
	public class CriticalDistanceBehaviour : RangedChasingBehaviour
	{
		[SerializeField] private float runAwayRange = 3f;

		public override void TriggerUpdate()
		{
			base.TriggerUpdate();

			if (IsWithinRunAwayRange)
			{
				MoveAwayFromPosition(TargetPosition);
			}
		}

		protected override bool ShouldTurnToFaceTarget
			=> base.ShouldTurnToFaceTarget && !IsWithinRunAwayRange;

		public bool IsWithinRunAwayRange => DistanceToTarget <= runAwayRange;
	}

}