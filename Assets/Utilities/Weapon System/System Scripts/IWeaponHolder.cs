using System;
using System.Collections.Generic;
using System.Numerics;

namespace WeaponSystem
{
	public interface IWeaponHolder
	{
		bool EquipWeapon(IWeapon weapon);
		bool EquipWeapon(IWeaponSlot slot, IWeapon weapon);
		bool UnequipWeapon(IWeapon weapon);
		event Action<IWeapon> OnWeaponEquipped, OnWeaponUnequipped;
		bool EmptySlotAtIndex(int index);
		bool EmptyAllSlots();
		bool IsEquipped(IWeapon weapon);
		IWeaponSlot FindEmptySlot { get; }
		IEnumerable<IWeapon> GetWeapons { get; }
		IWeaponSlot GetSlotAtIndex(int index);
		void AddSlot(IWeaponSlot slot);
	}
}