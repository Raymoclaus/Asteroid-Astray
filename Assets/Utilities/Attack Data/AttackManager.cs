using System.Collections.Generic;
using UnityEngine;

namespace AttackData
{
	[RequireComponent(typeof(Collider2D))]
	public class AttackManager : MonoBehaviour
	{
		private HashSet<AttackComponent> attackComponents = new HashSet<AttackComponent>();

		private void Awake()
		{
			foreach (AttackComponent component in GetComponents<AttackComponent>())
			{
				attackComponents.Add(component);
			}
		}

		private void Update() => TryDestroySelf();

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			Transform otherTransform = otherCollider.transform;
			foreach (AttackComponent component in attackComponents)
			{
				component.Contact(otherCollider);
			}

			while (otherTransform != null)
			{
				IAttackReceiver[] receivers = otherTransform.GetComponents<IAttackReceiver>();
				for (int i = 0; i < receivers.Length; i++)
				{
					IAttackReceiver receiver = receivers[i];
					if (receiver != null)
					{
						foreach (AttackComponent component in attackComponents)
						{
							if (!component.VerifyTarget(receiver)) continue;
						}

						receiver.ReceiveAttack(this);
					}
				}
				otherTransform = otherTransform.parent;
			}
		}

		public T AddAttackComponent<T>(object data = null) where T : AttackComponent
		{
			System.Type type = typeof(T);
			if (!type.IsSubclassOf(typeof(AttackComponent))) return null;
			T component = gameObject.AddComponent<T>();
			component.AssignData(data);
			attackComponents.Add(component);
			return component;
		}

		public T GetAttackComponent<T>() where T : AttackComponent
		{
			System.Type type = typeof(T);
			if (!type.IsSubclassOf(typeof(AttackComponent))) return null;
			foreach (AttackComponent component in attackComponents)
			{
				if (component is T) return (T)component;
			}
			return null;
		}

		public object GetData<T>() where T : AttackComponent => GetAttackComponent<T>()?.GetData();

		public void Hit(IAttackReceiver target)
		{
			foreach (AttackComponent component in attackComponents)
			{
				component.Hit(target);
			}
			TryDestroySelf();
		}

		private void TryDestroySelf()
		{
			foreach (AttackComponent component in attackComponents)
			{
				if (component.ShouldDestroy())
				{
					Destroy(gameObject);
					return;
				}
			}
		}
	}
}
