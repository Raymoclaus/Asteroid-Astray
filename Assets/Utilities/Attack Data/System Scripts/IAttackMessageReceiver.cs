using AttackData;

public interface IAttackMessageReceiver
{
	bool ReceiveAttack(AttackManager atkMngr);
	bool CanReceiveAttackMessagesFromLayer(int layer);
}
