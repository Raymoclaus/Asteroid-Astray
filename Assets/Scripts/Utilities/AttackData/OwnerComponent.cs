using UnityEngine;

namespace AttackData
{
	public class OwnerComponent : AttackComponent
	{
		//the character or script that fired the attack
		public MonoBehaviour owner;

		public override void AssignData(object data)
		{
			base.AssignData(data);
			owner = (MonoBehaviour)data;
		}

		public override object GetData() => owner;
	}
}
