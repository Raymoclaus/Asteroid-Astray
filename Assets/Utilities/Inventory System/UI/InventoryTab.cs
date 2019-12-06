using System.Collections.Generic;
using TabbedMenuSystem;
using UnityEngine;

namespace InventorySystem.UI
{
	public class InventoryTab : MenuContent
	{
		private static IInventoryHolder inventoryHolder;
		private static InventoryTab instance;
		
		[SerializeField] private SlotGroup slotGroupPrefab;
		private HashSet<SlotGroup> slotGroups = new HashSet<SlotGroup>();
		[SerializeField] Transform slotGroupsHolder;
		private Storage currentInventory;
		[SerializeField] private ItemStackUI grabStack;
		[SerializeField] private CraftingUIController craftingUI;
		[SerializeField] private ItemPreviewUI itemPreviewUI;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Destroy(gameObject);
				return;
			}
		}

		private void Update()
		{
			UpdateGrabUI();
		}

		public static void SetInventoryHolder(IInventoryHolder newInventoryHolder)
		{
			if (newInventoryHolder == null) return;
			inventoryHolder = newInventoryHolder;
		}

		public override void OnOpen()
		{
			base.OnOpen();

			foreach (Transform child in slotGroupsHolder)
			{
				Destroy(child.gameObject);
			}

			Storage defaultInventory = inventoryHolder?.DefaultInventory;
			CreateSlotGroup(defaultInventory);

			craftingUI.SetCrafter(inventoryHolder as ICrafter);
			craftingUI.Setup();

			itemPreviewUI.SetItemType(ItemObject.Blank);
		}

		public override void OnClose()
		{
			base.OnClose();

			ItemObject grabbedItem = grabStack.ItemType;
			if (grabbedItem == ItemObject.Blank) return;
			Storage inv = inventoryHolder.GetAppropriateInventory(grabbedItem);
			inv.AddItem(new ItemStack(grabStack.ItemType, grabStack.Amount));
			grabStack.SetStack(new ItemStack());
		}

		private void CreateSlotGroup(Storage inv)
		{
			if (inv == null) return;

			SlotGroup newGroup = Instantiate(slotGroupPrefab, slotGroupsHolder);
			newGroup.Initialise(this, inv);
			slotGroups.Add(newGroup);
		}

		private void UpdateGrabUI()
		{
			if (!IsGrabbing) return;
			Vector3 pos = Input.mousePosition;
			pos.z = grabStack.transform.parent.position.z;
			grabStack.transform.position = pos;
		}

		private void UpdatePreview(ItemObject type)
		{
			ItemObject previewType =
				IsGrabbing ? grabStack.ItemType : type;
			if (previewType == ItemObject.Blank) return;

			itemPreviewUI.SetItemType(previewType);
		}

		public void SlotHover(Slot slot)
		{
			ItemObject slotType = slot.ItemType;
			UpdatePreview(slotType);
		}

		public void SlotClickDown(Slot slot)
		{
			Storage inv = slot.Inventory;
			if (grabStack.ItemType == slot.ItemType
				&& !slot.IsMaxed)
			{
				int leftOver = inv.AddToStack(grabStack.ItemType, grabStack.Amount, slot.ID);
				grabStack.Amount = leftOver;
			}
			else
			{
				ItemStack swap = slot.Inventory.Replace(grabStack.StackCopy, slot.ID);
				grabStack.SetStack(swap);
			}
		}

		public Slot SlotLastClickedIn { get; set; }

		public bool IsGrabbing => grabStack.ItemType != ItemObject.Blank;
	}
}