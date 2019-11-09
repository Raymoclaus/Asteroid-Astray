using System;
using UnityEngine;

namespace WeaponSystem
{
	public class BasicWeaponSlot : MonoBehaviour, IWeaponSlot
	{
		public event Action<IWeapon> OnEquipped;
		public event Action<IWeapon> OnUnequipped;

		protected virtual void Awake()
		{
			IWeapon weapon = GetComponentInChildren<IWeapon>();
			Equip(weapon);
		}

		public IWeapon Weapon { get; private set; }

		public bool IsEmpty => Weapon == null;

		/// <summary>
		/// Gives the slot a reference to a weapon. Can't equip a weapon if already holding one.
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns>Returns whether the contents of the slot changed.</returns>
		public bool Equip(IWeapon weapon)
		{
			if (!IsEmpty || weapon == null) return false;
			Weapon = weapon;
			OnEquipped?.Invoke(weapon);
			return true;
		}

		public bool Unequip()
		{
			if (IsEmpty) return false;
			IWeapon prevWeapon = Weapon;
			Weapon = null;
			OnUnequipped?.Invoke(prevWeapon);
			return true;
		}
	}
}
