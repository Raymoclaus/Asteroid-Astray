using System;

namespace WeaponSystem
{
	public interface IWeaponSlot
	{
		IWeapon Weapon { get; }
		bool Equip(IWeapon weapon);
		bool Unequip();
		bool IsEmpty { get; }
		event Action<IWeapon> OnEquipped, OnUnequipped;
	}
}