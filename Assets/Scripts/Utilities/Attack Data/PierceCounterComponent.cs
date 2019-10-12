namespace AttackData
{
	public class PierceCounterComponent : AttackComponent
	{
		public int pierceCount;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			pierceCount = (int)data;
		}

		public override object GetData() => pierceCount;

		public override void Hit(IAttackReceiver target) => pierceCount--;

		public override bool ShouldDestroy() => pierceCount <= 0;
	}
}
