using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AttackData
{
	public class IsProjectileComponent : AttackComponent
	{
		bool isProjectile;

		public override void AssignData(object data)
			=> isProjectile = (bool)data;

		public override object GetData() => isProjectile;
	}
}
