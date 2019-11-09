using AttackData;
using TriggerSystem;

public interface IAttackActor : IActor
{
	AttackManager GetAttackManager { get; }
	void Hit(IAttackTrigger trigger);
}
