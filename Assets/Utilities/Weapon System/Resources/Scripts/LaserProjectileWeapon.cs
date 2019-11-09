using System.Collections.Generic;
using AttackData;
using UnityEngine;

namespace WeaponSystem
{
	public class LaserProjectileWeapon : BasicProjectileWeapon
	{
		[SerializeField] private string triggerAction = "Attack";
		[SerializeField] private Transform weaponPivot;

		public override string TriggerAction => triggerAction;

		public override AttackManager Attack(float damageMultiplier, List<IAttacker> owners)
		{
			AttackManager attack = base.Attack(damageMultiplier, owners);

			if (attack == null) return null;

			IAmmo ammo = (IAmmo) attack;
			ammo.SetInitialWeaponPosition(weaponPivot.position);

			return attack;
		}
	} 
}
