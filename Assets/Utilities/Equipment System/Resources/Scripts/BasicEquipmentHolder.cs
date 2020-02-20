using AttackData;
using InputHandlerSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EquipmentSystem
{
	public class BasicEquipmentHolder : MonoBehaviour, IEquipmentHolder
	{
		private List<IAttacker> owners = new List<IAttacker>();
		[SerializeField] private GameAction defaultConstantAction;
		[SerializeField] private float damageMultiplier = 1f;
		private const int INITIAL_SLOT_COUNT = 1;
		private List<IEquipmentSlot> slots = new List<IEquipmentSlot>();

		public event Action<IEquipment> OnComponentEquipped;
		public event Action<IEquipment> OnComponentUnequipped;

		protected virtual void Awake()
		{
			IEquipmentSlot[] childSlots = GetComponentsInChildren<IEquipmentSlot>();
			foreach (IEquipmentSlot slot in childSlots)
			{
				AddSlot(slot);
			}

			owners.AddRange(GetComponentsInParent<IAttacker>());
		}

		protected virtual void Update()
		{
			PerformAction(defaultConstantAction);

			foreach (ITriggerableEquipment weapon in GetAllTriggerrableEquipment)
			{
				GameAction action = weapon.TriggerAction;
				PerformAction(action);
			}
		}

		protected void PerformAction(GameAction action)
		{
			if (owners.Exists(t => !t.ShouldAttack(action))) return;

			Vector3 recoilVector = Vector3.zero;
			foreach (ITriggerableEquipment equipment in GetEquipmentWithTriggerAction(action))
			{
				if (equipment.Attack(damageMultiplier, owners) != null)
				{
					if (equipment is IProjectileEquipment pWeapon)
					{
						recoilVector += pWeapon.RecoilVector;
					}
				}
			}
			owners.ForEach(t => t.ReceiveRecoil(recoilVector));
		}

		private IEnumerable<ITriggerableEquipment> GetEquipmentWithTriggerAction(GameAction action)
		{
			return GetAllTriggerrableEquipment.Where(t => t.TriggerAction == action);
		}

		private void ComponentEquipped(IEquipment equipment)
			=> OnComponentEquipped?.Invoke(equipment);

		private void ComponentUnequipped(IEquipment equipment)
			=> OnComponentUnequipped?.Invoke(equipment);

		public bool IsEquipped(IEquipment equipment)
			=> GetMatchingSlot(equipment) != null;

		public IEquipmentSlot FindEmptySlot => AllEmptySlots.FirstOrDefault();

		private IEnumerable<IEquipmentSlot> AllEmptySlots
			=> slots.Where(t => t.IsEmpty);

		private IEnumerable<IEquipmentSlot> AllFilledSlots
			=> slots.Where(t => !t.IsEmpty);

		public bool EquipComponent(IEquipment equipment)
		{
			if (IsEquipped(equipment)) return false;
			IEquipmentSlot emptySlot = FindEmptySlot;
			return EquipComponent(emptySlot, equipment);
		}

		public bool UnequipComponent(IEquipment equipment)
		{
			IEquipmentSlot matchingSlot = GetMatchingSlot(equipment);
			if (matchingSlot == null) return false;
			return matchingSlot.Unequip();
		}

		public IEquipmentSlot GetMatchingSlot(IEquipment equipment)
			=> slots.FirstOrDefault(t => t.equipment == equipment);

		public IEnumerable<IEquipment> GetAllEquipment
			=> slots.Where(t => !t.IsEmpty).Select(t => t.equipment);

		public IEnumerable<ITriggerableEquipment> GetAllTriggerrableEquipment
			=> GetAllEquipment.Select(t => t as ITriggerableEquipment).Where(t => t != null);

		public void AddSlot(IEquipmentSlot slot)
		{
			if (slots.Contains(slot)) return;
			slots.Add(slot);
			SubscribeToSlotEvents(slot);
		}

		public void RemoveSlot(IEquipmentSlot slot)
		{
			if (slots.Remove(slot))
			{
				UnsubscribeFromSlotEvents(slot);
			}
		}

		private void SubscribeToSlotEvents(IEquipmentSlot slot)
		{
			slot.OnEquipped += ComponentEquipped;
			slot.OnUnequipped += ComponentUnequipped;
		}

		public void UnsubscribeFromSlotEvents(IEquipmentSlot slot)
		{
			slot.OnEquipped -= ComponentEquipped;
			slot.OnUnequipped -= ComponentUnequipped;
		}

		public bool OwnsSlot(IEquipmentSlot slot)
		{
			int index = GetSlotIndex(slot);
			return index >= 0 && index < SlotCount;
		}

		public bool EquipComponent(IEquipmentSlot slot, IEquipment equipment)
			=> OwnsSlot(slot) ? slot.Equip(equipment) : false;

		public bool EmptySlot(IEquipmentSlot slot)
		{
			if (!OwnsSlot(slot)) return false;
			return slot.Unequip();
		}

		public bool EmptySlotAtIndex(int index)
		{
			IEquipmentSlot slot = GetSlotAtIndex(index);
			if (slot == null) return false;
			return slot.Unequip();
		}

		public bool EmptyAllSlots() => slots.Count(t => t.Unequip()) > 0;

		public int GetSlotIndex(IEquipmentSlot slot)
			=> slot != null ? slots.IndexOf(slot) : -1;

		public IEquipmentSlot GetSlotAtIndex(int index)
			=> index >= 0 && index < SlotCount ? slots[index] : null;

		public int SlotCount => slots.Count;
	}
}
