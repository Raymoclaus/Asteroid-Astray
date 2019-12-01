namespace AttackData
{
	public class DamageMultiplierComponent : AttackComponent
	{
		public float multiplier = 1f;

		public override void AssignData(object data = null)
		{
			base.AssignData(data);

			if (data == null) return;
			if (data is float mult)
			{
				multiplier = mult;
			}
		}

		public override object GetData() => multiplier;
	}
}