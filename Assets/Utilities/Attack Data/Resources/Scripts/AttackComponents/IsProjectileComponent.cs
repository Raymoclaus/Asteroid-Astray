namespace AttackData
{
	public class IsProjectileComponent : AttackComponent
	{
		public bool isProjectile = true;

		public override void AssignData(object data)
			=> isProjectile = (bool)data;

		public override object GetData() => isProjectile;
	}
}
