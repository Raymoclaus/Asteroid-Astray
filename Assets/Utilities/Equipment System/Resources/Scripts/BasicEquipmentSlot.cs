using System;
using UnityEngine;

namespace EquipmentSystem
{
	public class BasicEquipmentSlot : MonoBehaviour, IEquipmentSlot
	{
		public event Action<IEquipment> OnEquipped;
		public event Action<IEquipment> OnUnequipped;

		protected virtual void Awake()
		{
			IEquipment equipment = GetComponentInChildren<IEquipment>();
			Equip(equipment);
		}

		public IEquipment equipment { get; private set; }

		public bool IsEmpty => equipment == null;

		/// <summary>
		/// Gives the slot a reference to a piece of equipment. Can't equip an item if already holding one.
		/// </summary>
		/// <param name="equipment"></param>
		/// <returns>Returns whether the contents of the slot changed.</returns>
		public bool Equip(IEquipment equipment)
		{
			if (!IsEmpty || equipment == null) return false;
			this.equipment = equipment;
			OnEquipped?.Invoke(equipment);
			return true;
		}

		public bool Unequip()
		{
			if (IsEmpty) return false;
			IEquipment prevEquipment = equipment;
			equipment = null;
			OnUnequipped?.Invoke(prevEquipment);
			return true;
		}
	}
}
