﻿using System.Collections.Generic;
using AttackData;
using InputHandlerSystem;
using UnityEngine;

namespace EquipmentSystem
{
	public class LaserProjectileWeapon : BasicProjectileWeapon
	{
		[SerializeField] private InputAction triggerAction;
		[SerializeField] private Transform centrePivot;

		public override InputAction TriggerAction => triggerAction;

		public override AttackManager Attack(float damageMultiplier, List<IAttacker> owners)
		{
			AttackManager attack = base.Attack(damageMultiplier, owners);

			if (attack == null) return null;

			IAmmo ammo = (IAmmo) attack;
			ammo.SetInitialWeaponPivot(centrePivot.position);

			return attack;
		}
	} 
}
