using UnityEngine;

namespace AttackData
{
	public abstract class AttackComponent : MonoBehaviour
	{
		public virtual void AssignData(object data = null)
		{

		}

		public virtual object GetData() => null;

		public virtual void Hit(IAttackTrigger target) { }

		public virtual bool ShouldDestroy() => false;

		public virtual bool VerifyTarget(IAttackTrigger target) => true;
	}
}
