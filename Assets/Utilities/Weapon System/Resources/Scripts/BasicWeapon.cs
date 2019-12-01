using AttackData;
using AudioUtilities;
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
		[SerializeField] private AudioSO clip;
		private static AudioManager audioMngr;

		private static AudioManager AudioMngr =>
			audioMngr != null
				? audioMngr
				: (audioMngr = FindObjectOfType<AudioManager>());

		protected virtual void Awake()
		{
			cooldownComponent.SetToLowerLimit();
		}

		private void FixedUpdate()
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

		public virtual bool ShouldAttack
			=> !IsOnCooldown
			   && gameObject.activeInHierarchy;

		public float CooldownDuration => cooldownComponent.UpperLimit;

		public bool IsOnCooldown => cooldownComponent.CurrentRatio > 0f;

		public float DamageMultiplier => damageMultiplier;

		protected virtual Vector3 WeaponPosition
			=> transform.position;

		public virtual AttackManager Attack(float damageMultiplier, List<IAttacker> owners)
		{
			if (!ShouldAttack) return null;
			AttackManager attack = SpawnAttack();
			attack.SetData<OwnerComponent>(owners, true);
			DamageComponent damageComponent = attack.GetAttackComponent<DamageComponent>();
			if (damageComponent != null)
			{
				damageComponent.Multiply(DamageMultiplier);
			}
			cooldownComponent.SetValue(CooldownDuration);
			owners.ForEach(t => t.ReceiveRecoveryDuration(recoveryDuration));
			AudioMngr.PlaySFX(clip, transform.position, null);
			return attack;
		}

		protected virtual AttackManager SpawnAttack()
		{
			return Instantiate(attackPrefab, WeaponPosition,
				Quaternion.identity, null);
		}

		public virtual string TriggerAction => "Attack";
	}
}