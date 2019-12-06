using UnityEngine;

namespace AttackData
{
	public interface IAmmo : IPoolable, IAttackActor
	{
		void MultiplyDamage(float multiplier);
		void SetIntialVelocity(Vector3 velocity);
		void SetInitialWeaponDirection(Vector3 direction);
		void SetInitialWeaponPivot(Vector3 position);
	} 
}
