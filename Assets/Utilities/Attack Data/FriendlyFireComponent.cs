namespace AttackData
{
	public class FriendlyFireComponent : AttackComponent
	{
		private bool canFriendlyFire;

		public override void AssignData(object data)
			=> canFriendlyFire = (bool)data;

		public override object GetData() => canFriendlyFire;
	}
}
