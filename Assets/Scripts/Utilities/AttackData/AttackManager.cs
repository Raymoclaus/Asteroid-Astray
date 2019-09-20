using System.Collections.Generic;
using UnityEngine;

namespace AttackData
{
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

		public T AddAttackComponent<T>(object data) where T : AttackComponent
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

		public void Hit()
		{
			foreach (AttackComponent component in attackComponents)
			{
				component.Hit();
			}
		}
	}
}
