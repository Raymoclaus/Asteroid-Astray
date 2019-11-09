using AttackData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeaponSystem
{
	public class BasicWeaponHolder : MonoBehaviour, IWeaponHolder
	{
		private List<IAttacker> owners = new List<IAttacker>();
		[SerializeField] private string defaultConstantAction = "Attack";
		[SerializeField] private float damageMultiplier = 1f;
		private const int INITIAL_SLOT_COUNT = 1;
		private List<IWeaponSlot> slots = new List<IWeaponSlot>();

		public event Action<IWeapon> OnWeaponEquipped;
		public event Action<IWeapon> OnWeaponUnequipped;

		protected virtual void Awake()
		{
			IWeaponSlot[] childSlots = GetComponentsInChildren<IWeaponSlot>();
			foreach (IWeaponSlot slot in childSlots)
			{
				AddSlot(slot);
			}

			owners.AddRange(GetComponentsInParent<IAttacker>());
		}

		protected virtual void Update()
		{
			PerformAction(defaultConstantAction);
		}

		protected void PerformAction(string action)
		{
			if (owners.Exists(t => !t.ShouldAttack(action))) return;

			foreach (IWeapon weapon in GetWeaponsWithTriggerAction(action))
			{
				weapon.Attack(damageMultiplier, owners);
			}
		}

		private IEnumerable<IWeapon> GetWeaponsWithTriggerAction(string action)
		{
			return GetWeapons.Where(t => t.TriggerAction == action);
		}

		private void WeaponEquipped(IWeapon weapon)
			=> OnWeaponEquipped?.Invoke(weapon);

		private void WeaponUnequipped(IWeapon weapon)
			=> OnWeaponUnequipped?.Invoke(weapon);

		public bool IsEquipped(IWeapon weapon)
			=> GetMatchingSlot(weapon) != null;

		public IWeaponSlot FindEmptySlot => AllEmptySlots.FirstOrDefault();

		private IEnumerable<IWeaponSlot> AllEmptySlots
			=> slots.Where(t => t.IsEmpty);

		private IEnumerable<IWeaponSlot> AllFilledSlots
			=> slots.Where(t => !t.IsEmpty);

		public bool EquipWeapon(IWeapon weapon)
		{
			if (IsEquipped(weapon)) return false;
			IWeaponSlot emptySlot = FindEmptySlot;
			return EquipWeapon(emptySlot, weapon);
		}

		public bool UnequipWeapon(IWeapon weapon)
		{
			IWeaponSlot matchingSlot = GetMatchingSlot(weapon);
			if (matchingSlot == null) return false;
			return matchingSlot.Unequip();
		}

		public IWeaponSlot GetMatchingSlot(IWeapon weapon)
			=> slots.FirstOrDefault(t => t.Weapon == weapon);

		public IEnumerable<IWeapon> GetWeapons
			=> slots.Where(t => !t.IsEmpty).Select(t => t.Weapon);

		public void AddSlot(IWeaponSlot slot)
		{
			if (slots.Contains(slot)) return;
			slots.Add(slot);
			SubscribeToSlotEvents(slot);
		}

		public void RemoveSlot(IWeaponSlot slot)
		{
			if (slots.Remove(slot))
			{
				UnsubscribeFromSlotEvents(slot);
			}
		}

		private void SubscribeToSlotEvents(IWeaponSlot slot)
		{
			slot.OnEquipped += WeaponEquipped;
			slot.OnUnequipped += WeaponUnequipped;
		}

		public void UnsubscribeFromSlotEvents(IWeaponSlot slot)
		{
			slot.OnEquipped -= WeaponEquipped;
			slot.OnUnequipped -= WeaponUnequipped;
		}

		public bool OwnsSlot(IWeaponSlot slot)
		{
			int index = GetSlotIndex(slot);
			return index >= 0 && index < SlotCount;
		}

		public bool EquipWeapon(IWeaponSlot slot, IWeapon weapon)
			=> OwnsSlot(slot) ? slot.Equip(weapon) : false;

		public bool EmptySlot(IWeaponSlot slot)
		{
			if (!OwnsSlot(slot)) return false;
			return slot.Unequip();
		}

		public bool EmptySlotAtIndex(int index)
		{
			IWeaponSlot slot = GetSlotAtIndex(index);
			if (slot == null) return false;
			return slot.Unequip();
		}

		public bool EmptyAllSlots() => slots.Count(t => t.Unequip()) > 0;

		public int GetSlotIndex(IWeaponSlot slot)
			=> slot != null ? slots.IndexOf(slot) : -1;

		public IWeaponSlot GetSlotAtIndex(int index)
			=> index >= 0 && index < SlotCount ? slots[index] : null;

		public int SlotCount => slots.Count;
	}
}
