using UnityEngine;

namespace AttackData
{
	public abstract class AttackComponent : MonoBehaviour
	{
		public virtual void AssignData(object data)
		{

		}

		public virtual object GetData() => null;

		public virtual void Hit() { }
	}
}
