using System.Collections.Generic;

namespace AttackData
{
	public class OwnerComponent : AttackComponent
	{
		//the character or script that fired the attack
		public List<IAttacker> owners = new List<IAttacker>();

		public override void AssignData(object data)
		{
			base.AssignData(data);
			if (data is IAttacker attacker)
			{
				owners.Clear();
				owners.Add(attacker);
			}
			else if (data is List<IAttacker> attackers)
			{
				owners.Clear();
				owners.AddRange(attackers);
			}
		}

		public override object GetData() => owners;
	}
}
