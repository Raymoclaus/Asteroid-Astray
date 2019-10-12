namespace AttackData
{
	public interface IAttackReceiver
	{
		void ReceiveAttack(AttackManager atkM);
		string LayerName { get; }
	}
}
