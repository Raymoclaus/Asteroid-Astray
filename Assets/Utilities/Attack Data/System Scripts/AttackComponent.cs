using UnityEngine;

namespace AttackData
{
	[RequireComponent(typeof(AttackManager))]
	public abstract class AttackComponent : MonoBehaviour
	{
		private AttackManager atkMngr;
		protected AttackManager AtkMngr
			=> atkMngr != null
				? atkMngr
				: (atkMngr = GetComponent<AttackManager>());

		public virtual void AssignData(object data = null)
		{

		}

		public virtual object GetData() => null;

		public virtual void Hit(IAttackTrigger target) { }

		public virtual bool ShouldDestroy() => false;

		public virtual bool VerifyTarget(IAttackTrigger target) => true;
	}
}
