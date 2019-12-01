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

		public float DamageIncludingBonuses
			=> damage * Multiplier.multiplier;

		public void Multiply(float multiplierValue)
		{
			Multiplier.multiplier *= multiplierValue;
		}

		private DamageMultiplierComponent multiplier;
		private DamageMultiplierComponent Multiplier
			=> multiplier != null
				? multiplier
				: (multiplier = AtkMngr.GetAttackComponent<DamageMultiplierComponent>(true));
	}
}
