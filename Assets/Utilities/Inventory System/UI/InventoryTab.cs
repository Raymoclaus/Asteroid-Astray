using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
	using TabbedMenuSystem;

	public class InventoryTab : MenuContent
	{
		private static IInventoryHolder inventoryHolder;
		private static InventoryTab instance;
		
		[SerializeField] private SlotGroup slotGroupPrefab;
		private HashSet<SlotGroup> slotGroups = new HashSet<SlotGroup>();
		[SerializeField] Transform slotGroupsHolder;
		private Inventory currentInventory;
		private bool grabbing = false;
		[SerializeField] private ItemStackUI grabStack;
		[SerializeField] private CraftingUIController craftingUI;

		[SerializeField] private Image previewImg;
		[SerializeField] private Text previewName;
		[SerializeField] private Text previewDesc;
		[SerializeField] private Text previewFlav;
		[SerializeField] private ItemSprites sprites;

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

			Inventory defaultInventory = inventoryHolder?.DefaultInventory;
			CreateSlotGroup(defaultInventory);

			craftingUI.SetCrafter(inventoryHolder as ICrafter);
			craftingUI.Setup();
		}

		public override void OnClose()
		{
			base.OnClose();

			Item.Type grabbedItem = grabStack.ItemType;
			if (grabbedItem == Item.Type.Blank) return;
			Inventory inv = inventoryHolder.GetAppropriateInventory(grabbedItem);
			inv.AddItem(new ItemStack(grabStack.ItemType, grabStack.Amount));
			grabStack.SetStack(new ItemStack());
		}

		private void CreateSlotGroup(Inventory inv)
		{
			if (inv == null) return;

			SlotGroup newGroup = Instantiate(slotGroupPrefab, slotGroupsHolder);
			newGroup.SetInventory(inv);
			slotGroups.Add(newGroup);
		}

		private void UpdateGrabUI()
		{
			if (!grabbing) return;
			Vector3 pos = Input.mousePosition;
			pos.z = grabStack.transform.parent.position.z;
			grabStack.transform.position = pos;
		}

		//private void UpdatePreview()
		//{
		//	Item.Type previewType =
		//		grabbing ? grabStack.ItemType : GetInventory(currentContext.context)
		//		.stacks[currentContext.selected].GetItemType();
		//	if (previewType == Item.Type.Blank) return;

		//	if (sprites != null)
		//	{
		//		previewImg.sprite = sprites.GetItemSprite(previewType);
		//	}
		//	previewImg.color = previewType == Item.Type.Blank ? Color.clear : Color.white;
		//	previewName.text = Item.TypeName(previewType);
		//	previewDesc.text = Item.ItemDescription(previewType);
		//	previewFlav.text = Item.ItemFlavourText(previewType);
		//}

		//public void PointerEnter(GameObject slot)
		//{
		//	UpdateSelected(slot);
		//	UpdatePreview();
		//}

		public void SlotClickDown(Slot slot)
		{
			Inventory inv = slot.Inventory;
			if (grabStack.ItemType == slot.ItemType
				&& !slot.IsMaxed)
			{
				int leftOver = inv.AddToStack(slot.ItemType, slot.Amount, slot.ID);
				grabStack.Amount = leftOver;
			}
			else
			{
				ItemStack swap = slot.Inventory.Replace(grabStack.StackCopy, slot.ID);
				grabStack.SetStack(swap);
			}
			grabbing = grabStack.ItemType != Item.Type.Blank;
		}
	}
}