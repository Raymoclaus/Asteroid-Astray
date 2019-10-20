using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InventorySystem.UI
{
	public class InventoryUIController : PauseTab
	{
		private static IInventoryHolder inventoryHolder;
		private static InventoryUIController instance;

		private ICrafter crafter;
		[SerializeField] private SlotGroup slotGroupPrefab;
		private HashSet<SlotGroup> slotGroups = new HashSet<SlotGroup>();
		[SerializeField] Transform slotGroupsHolder;
		private Inventory currentInventory;
		private bool grabbing = false;
		[SerializeField] private ItemStackUI grabStack;

		[SerializeField] private Image previewImg;
		[SerializeField] private Text previewName;
		[SerializeField] private Text previewDesc;
		[SerializeField] private Text previewFlav;
		[SerializeField] private ItemSprites sprites;

		[SerializeField] private Transform recipeContent;
		[SerializeField] private RecipeUIObject recipeObjectPrefab;
		[SerializeField] private GameObject constructorUIObject;
		[SerializeField] private GameObject craftingDisabledObject;
		[SerializeField] private TextMeshProUGUI craftingDisabledReason;

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

			//crafter = inventoryHolder as ICrafter;

			////disable crafting UI if these conditions are met
			//if (crafter == null || crafter.GetRecipeStorage == null)
			//{
			//	craftingDisabledObject.SetActive(true);
			//	constructorUIObject.SetActive(false);
			//	craftingDisabledReason.text = "Unit cannot craft";
			//}
			//else if (crafter.GetRecipeStorage.RecipeCount == 0)
			//{
			//	craftingDisabledObject.SetActive(true);
			//	constructorUIObject.SetActive(false);
			//	craftingDisabledReason.text = "Unit has no crafting recipes";
			//}
			//else
			//{
			//	craftingDisabledObject.SetActive(false);
			//	constructorUIObject.SetActive(true);

			//	foreach (Transform child in recipeContent)
			//	{
			//		Destroy(child.gameObject);
			//	}

			//	//fill up recipe area with units currently accessible recipes
			//	List<CraftingRecipe> recipes = crafter.GetRecipeStorage.recipes;

			//	for (int i = 0; i < recipes.Count; i++)
			//	{
			//		RecipeUIObject recipeObject = Instantiate(
			//			recipeObjectPrefab, recipeContent);
			//		recipeObject.Initialise(recipes[i]);
			//	}
			//}
		}

		public override void OnResume()
		{
			base.OnResume();

			Item.Type grabbedItem = grabStack.ItemType;
			if (grabbedItem == Item.Type.Blank) return;
			Inventory inv = inventoryHolder.GetAppropriateInventory(grabbedItem);
			inv.AddItem(new ItemStack(grabStack.ItemType, grabStack.Amount));
			grabStack.SetStack(new ItemStack());
		}

		private void Update()
		{
			UpdateGrabUI();
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