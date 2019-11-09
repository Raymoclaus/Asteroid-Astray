using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TriggerSystem;
using UnityEngine;

namespace AttackData
{
	[RequireComponent(typeof(Collider2D))]
	public class AttackManager : MonoBehaviour, IAttackActor, IPoolable
	{
		private HashSet<AttackComponent> attackComponents = new HashSet<AttackComponent>();

		public event Action<IActor> OnDisabled;
		public event Action<IPoolable> OnReturnToPool;

		public bool IsAttachedToPool { get; set; }

		public bool CanTriggerPrompts => false;

		public AttackManager GetAttackManager => this;

		private Rigidbody2D rb;
		private Rigidbody2D Rb => rb != null ? rb
			: (rb = GetComponent<Rigidbody2D>());

		public Vector3 Position
		{
			get => transform.position;
			set
			{
				transform.position = value;
			}
		}

		protected virtual void Awake()
		{
			foreach (AttackComponent component in GetComponents<AttackComponent>())
			{
				attackComponents.Add(component);
			}
		}

		protected virtual void Update() => TryDestroySelf();

		private void OnDisable() => OnDisabled?.Invoke(this);

		public T AddAttackComponent<T>(object data = null) where T : AttackComponent
		{
			Type type = typeof(T);
			Type baseType = typeof(AttackComponent);
			if (!type.IsSubclassOf(baseType))
			{
				Debug.LogWarning($"T parameter must be a subclass of {baseType}. {baseType} is abstract.", gameObject);
				return null;
			}

			T component = gameObject.AddComponent<T>();
			component.AssignData(data);
			attackComponents.Add(component);
			return component;
		}

		public T GetAttackComponent<T>() where T : AttackComponent
		{
			Type type = typeof(T);
			if (!type.IsSubclassOf(typeof(AttackComponent))) return null;
			foreach (AttackComponent component in attackComponents)
			{
				if (component is T) return (T)component;
			}
			return null;
		}

		public object GetData<T>() where T : AttackComponent
			=> GetAttackComponent<T>()?.GetData();

		public void SetData<T>(object data, bool createComponentIfNoneFound) where T : AttackComponent
		{
			T comp = GetAttackComponent<T>();
			if (comp == null && createComponentIfNoneFound)
			{
				comp = AddAttackComponent<T>();
			}

			if (comp != null)
			{
				comp.AssignData(data);
			}
		}

		public void Hit(IAttackTrigger trigger)
		{
			foreach (AttackComponent component in attackComponents)
			{
				if (!component.VerifyTarget(trigger)) return;
			}

			foreach (AttackComponent component in attackComponents)
			{
				component.Hit(trigger);
			}

			TryDestroySelf();
		}

		private void TryDestroySelf()
		{
			foreach (AttackComponent component in attackComponents)
			{
				if (component.ShouldDestroy())
				{
					if (IsAttachedToPool)
					{
						gameObject.SetActive(false);
						OnReturnToPool?.Invoke(this);
					}
					else
					{
						Destroy(gameObject);
					}
					return;
				}
			}
		}

		public void EnteredTrigger(ITrigger vTrigger)
		{

		}

		public void ExitedTrigger(ITrigger vTrigger)
		{

		}
	}
}
