using System;

namespace EquipmentSystem
{
	public interface IEquipmentSlot
	{
		IEquipment equipment { get; }
		bool Equip(IEquipment equipment);
		bool Unequip();
		bool IsEmpty { get; }
		event Action<IEquipment> OnEquipped, OnUnequipped;
	}
}