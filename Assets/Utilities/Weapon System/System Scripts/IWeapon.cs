using AttackData;
using System.Collections.Generic;
using System.Numerics;

namespace WeaponSystem
{
	public interface IWeapon
	{
		AttackManager GetAttack { get; }
		AttackManager Attack(float damageMultiplier, List<IAttacker> owners);
		bool ShouldAttack { get; }
		float CooldownDuration { get; }
		bool IsOnCooldown { get; }
		string TriggerAction { get; }
	}
}