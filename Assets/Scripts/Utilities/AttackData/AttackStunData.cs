namespace AttackData
{
	public class AttackStunData : AttackComponent
	{
		public float stunDuration;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			stunDuration = (float)data;
		}

		public override object GetData() => stunDuration;
	}
}
