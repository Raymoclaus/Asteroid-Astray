using System.Collections.Generic;
using AttackData;
using InputHandlerSystem;

namespace EquipmentSystem
{
	public interface ITriggerableEquipment : IEquipment
	{
		AttackManager GetAttack { get; }
		AttackManager Attack(float damageMultiplier, List<IAttacker> owners);
		bool ShouldAttack { get; }
		float CooldownDuration { get; }
		bool IsOnCooldown { get; }
		GameAction TriggerAction { get; }
	} 
}
