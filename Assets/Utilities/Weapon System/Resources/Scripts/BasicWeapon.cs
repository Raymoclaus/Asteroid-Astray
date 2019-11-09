using AttackData;
using System.Collections.Generic;
using UnityEngine;
using ValueComponents;

namespace WeaponSystem
{
	public class BasicWeapon : MonoBehaviour, IWeapon
	{
		[SerializeField] protected AttackManager attackPrefab;
		[SerializeField] protected RangedFloatComponent cooldownComponent;
		[SerializeField] private float recoveryDuration;
		[SerializeField] protected float damageMultiplier = 1f;

		protected virtual void Awake()
		{
			cooldownComponent.SetToLowerLimit();
		}

		protected virtual void Update()
		{
			float reduction = Time.deltaTime;
			ReduceCooldownBySeconds(reduction);
		}

		public void ReduceCooldownBySeconds(float reduction)
			=> cooldownComponent.SubtractValue(reduction);

		public void ReduceCooldownByPercentageOfTotalDuration(float percentage)
			=> cooldownComponent.SubtractRatio(percentage);

		public void ReduceCooldownByPercentageOfRemainingDuration(float percentage)
			=> cooldownComponent.MultiplyRatio(1f - percentage);

		public virtual AttackManager GetAttack => attackPrefab;

		public virtual bool ShouldAttack => !IsOnCooldown;

		public float CooldownDuration => cooldownComponent.UpperLimit;

		public bool IsOnCooldown => cooldownComponent.CurrentRatio > 0f;

		public float DamageMultiplier => damageMultiplier;

		public virtual AttackManager Attack(float damageMultiplier, List<IAttacker> owners)
		{
			if (!ShouldAttack) return null;

			AttackManager attack = Instantiate(attackPrefab, transform.position,
				Quaternion.identity, null);
			attack.SetData<OwnerComponent>(owners, true);
			attack.SetData<DamageComponent>(DamageMultiplier, false);
			cooldownComponent.SetValue(CooldownDuration);
			owners.ForEach(t => t.ReceiveRecoveryDuration(recoveryDuration));
			return attack;
		}

		public virtual string TriggerAction => "Attack";
	}
}