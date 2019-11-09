using AttackData;
using System.Collections.Generic;
using UnityEngine;
using ValueComponents;

namespace WeaponSystem
{
	public class BasicProjectileWeapon : BasicWeapon, IProjectileWeapon
	{
		[SerializeField] private RangedIntComponent clipComponent;
		[SerializeField] private RangedFloatComponent reloadComponent;
		[SerializeField] private float fireSpeed;
		[SerializeField] private float recoilAmount;
		[SerializeField] private float stoppingPower = 0.8f;
		[SerializeField] private GameObject muzzleFlashEffect;

		protected override void Awake()
		{
			base.Awake();
			AmmunitionPool = new ObjectPool<IAmmo>(
				() => (IAmmo)SpawnAttack());
			clipComponent.SetToUpperLimit();
			reloadComponent.SetToLowerLimit();
		}

		public new IAmmo GetAttack
		{
			get
			{
				AttackManager atk = base.GetAttack;
				if (atk is IAmmo) return (IAmmo) atk;
				Debug.LogWarning($"Attack Prefab class must inherit {typeof(IAmmo)}.", gameObject);
				return null;
			}
		}

		public ObjectPool<IAmmo> AmmunitionPool { get; private set; }

		public int ClipSize => clipComponent.UpperLimit;

		public float ReloadDuration => reloadComponent.UpperLimit;

		public IDirectionalObject DirectionalObject { get; set; }

		protected Vector3 Direction => transform.up;

		public Vector3 RecoilVector => -Direction * recoilAmount;

		public bool IsCompatible(IAmmo ammo) => true;

		public virtual Vector3 FireVelocity
			=> Direction * fireSpeed;

		public override AttackManager Attack(float damageMultiplier, List<IAttacker> owners)
		{
			if (GetAttack == null) return null;

			AttackManager attack = base.Attack(damageMultiplier, owners);
			if (attack == null) return null;

			IAmmo ammo = (IAmmo)attack;
			
			ammo.SetInitialWeaponDirection(DirectionalObject.Direction);
			ammo.SetIntialVelocity(FireVelocity);
			ammo.SetInitialWeaponPosition(WeaponPosition);
			owners.ForEach(t => t.ReceiveStoppingPower(stoppingPower));

			Instantiate(muzzleFlashEffect, WeaponPosition, transform.rotation, null);

			return ammo.GetAttackManager;
		}
	}
}