using UnityEngine;

namespace AttackData
{
	public class DirectionComponent : AttackComponent
	{
		public Vector3 direction;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			direction = (Vector3)data;
		}

		public override object GetData() => direction;
	}
}
