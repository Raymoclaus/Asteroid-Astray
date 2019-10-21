using UnityEngine;

namespace AttackData
{
	public abstract class AttackComponent : MonoBehaviour
	{
		public virtual void AssignData(object data = null)
		{

		}

		public virtual object GetData() => null;

		public virtual void Hit(IAttackReceiver target) { }

		public virtual bool ShouldDestroy() => false;

		public virtual void Contact(Collider2D target) { }

		public virtual bool VerifyTarget(IAttackReceiver target) => true;
	}
}
