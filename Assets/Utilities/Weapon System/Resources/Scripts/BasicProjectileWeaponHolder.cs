using UnityEngine;

namespace WeaponSystem
{
	public class BasicProjectileWeaponHolder : BasicWeaponHolder, IDirectionalObject
	{
		protected override void Awake()
		{
			base.Awake();
			foreach (IProjectileWeapon weapon in GetWeapons)
			{
				AddDirectionalObjectToProjectileWeapon(weapon);
			}
			OnWeaponEquipped += AddDirectionalObjectToProjectileWeapon;
			OnWeaponUnequipped += RemoveDirectionalObjectFromProjectileWeapon;
		}

		protected override void Update()
		{
			foreach (IWeapon weapon in GetWeapons)
			{
				string action = weapon.TriggerAction;
				PerformAction(action);
			}
		}

		private void AddDirectionalObjectToProjectileWeapon(IWeapon obj)
		{
			if (obj is IProjectileWeapon pWeapon)
			{
				pWeapon.DirectionalObject = this;
			}
		}

		private void RemoveDirectionalObjectFromProjectileWeapon(IWeapon obj)
		{
			if (obj is IProjectileWeapon pWeapon)
			{
				IDirectionalObject dirObj = this;
				if (pWeapon.DirectionalObject == dirObj)
				{
					pWeapon.DirectionalObject = null;
				}
			}
		}

		public Vector3 Direction => transform.up;
	}
}
