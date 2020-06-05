using AttackData;
using UnityEngine;

namespace MovementBehaviours
{
	public class RollTowardsProjectileBehaviour : MovementBehaviour, IAttackTrigger
	{
		public void ReceiveAttack(AttackManager atkM)
		{
			object isProjectile = atkM.GetData<IsProjectileComponent>();
			if (isProjectile == null || !(bool)isProjectile) return;
			DetectedProjectile(atkM);
		}

		public string LayerName => LayerMask.LayerToName(gameObject.layer);

		private void DetectedProjectile(AttackManager atkM)
		{
			Vector3 direction;
			object directionObj = atkM.GetData<VelocityComponent>();
			directionObj = directionObj
				?? atkM.GetData<DirectionComponent>()
				?? atkM.GetData<VelocityComponent>();
			direction = directionObj != null ? -(Vector3)directionObj : Vector3.up;
			TriggerRoll(direction.normalized);
		}
	}
}
