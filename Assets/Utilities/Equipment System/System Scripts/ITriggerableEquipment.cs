using AttackData;
using InputHandlerSystem;
using System.Collections.Generic;

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
