using System.Collections.Generic;

namespace AttackData
{
	public class OwnerComponent : AttackComponent
	{
		//the character or script that fired the attack
		public List<IAttacker> owners;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			owners = (List<IAttacker>)data;
		}

		public override object GetData() => owners;
	}
}
