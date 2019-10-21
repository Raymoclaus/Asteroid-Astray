namespace AttackData
{
	public class DamageComponent : AttackComponent
	{
		public float damage;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			damage = (float)data;
		}

		public override object GetData() => damage;
	}
}
