namespace AttackData
{
	public class AttackPierceData : AttackComponent
	{
		public int pierceCount;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			pierceCount = (int)data;
		}

		public override object GetData() => pierceCount;

		public override void Hit() => pierceCount--;
	}
}
