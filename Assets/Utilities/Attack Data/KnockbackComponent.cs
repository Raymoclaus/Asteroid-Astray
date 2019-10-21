using UnityEngine;

namespace AttackData
{
	public class KnockbackComponent : AttackComponent
	{
		public Vector3 knockBack;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			knockBack = (Vector3)data;
		}

		public override object GetData() => knockBack;
	}
}
