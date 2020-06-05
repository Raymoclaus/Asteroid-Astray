using System;
using System.Collections.Generic;

namespace EquipmentSystem
{
	public interface IEquipmentHolder
	{
		bool EquipComponent(IEquipment equipment);
		bool EquipComponent(IEquipmentSlot slot, IEquipment equipment);
		bool UnequipComponent(IEquipment equipment);
		event Action<IEquipment> OnComponentEquipped, OnComponentUnequipped;
		bool EmptySlotAtIndex(int index);
		bool EmptyAllSlots();
		bool IsEquipped(IEquipment equipment);
		IEquipmentSlot FindEmptySlot { get; }
		IEnumerable<IEquipment> GetAllEquipment { get; }
		IEquipmentSlot GetSlotAtIndex(int index);
		void AddSlot(IEquipmentSlot slot);
	}
}