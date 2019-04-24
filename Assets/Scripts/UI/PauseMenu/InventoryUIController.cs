using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : PauseTab
{
	[SerializeField] private SlotGroup mainGroup, craftingInputGroup, craftingOutputGroup,
		ghostInputGroup, ghostOutputGroup;

	private SelectionContext currentContext = new SelectionContext(ContextInterface.Inventory, 0);

	private ItemStack grabStack = new ItemStack(Item.Type.Blank, 0);
	private bool grabbing = false;
	[SerializeField] private Transform grabTransform;
	[SerializeField] private Image grabImg;
	[SerializeField] private Text grabCountText;

	[SerializeField] private Image previewImg;
	[SerializeField] private Text previewName;
	[SerializeField] private Text previewDesc;
	[SerializeField] private Text previewFlav;
	[SerializeField] private ItemSprites sprites;

	private CraftingRecipe currentRecipe;
	[HideInInspector] private CraftingRecipe ghostRecipe;
	[SerializeField] private GameObject recipeGroup, constructorGroup;
	[SerializeField] private Transform recipeContent;
	private List<CraftingRecipeSO> availableRecipes = new List<CraftingRecipeSO>();
	[SerializeField] private GameObject recipeObjectPrefab;

	private void Awake()
	{
		mainGroup.FindImagesAndTexts();
		craftingInputGroup.FindImagesAndTexts();
		craftingOutputGroup.FindImagesAndTexts();
		ghostInputGroup.FindImagesAndTexts();
		ghostOutputGroup.FindImagesAndTexts();
	}

	private void Update()
	{
		UpdateSlots();
		UpdateGrabUI();
		UpdatePreview();
	}

	public override void OnResume()
	{
		Inventory inv = craftingInputGroup.inventory;
		if (inv.HasItems())
		{
			mainGroup.inventory.AddItems(inv.stacks);
			inv.ClearAll();
		}
	}

	private void UpdateSlots()
	{
		UpdateSlotGroup(mainGroup, 1f);
		UpdateSlotGroup(craftingInputGroup, 1f);
		UpdateSlotGroup(craftingOutputGroup, 1f);
		UpdateSlotGroup(ghostInputGroup, 0.8f);
		UpdateSlotGroup(ghostOutputGroup, 0.8f);
	}

	private void UpdateSlotGroup(SlotGroup sg, float alpha)
	{
		Inventory inv = sg.inventory;
		List<ItemStack> stacks = inv.stacks;
		for (int i = 0; i < stacks.Count; i++)
		{
			Item.Type type = stacks[i].GetItemType();
			if (sprites)
			{
				sg.slotImages[i].sprite = sprites.GetItemSprite(type);
			}
			Color col = type == Item.Type.Blank ? Color.clear : new Color(1f, 1f, 1f, alpha);
			sg.slotImages[i].color = col;
			int count = stacks[i].GetAmount();
			sg.slotTexts[i].text = count > 1 ? count.ToString() : string.Empty;
		}
	}

	public void SetGhostRecipe(CraftingRecipe recipe)
	{
		ghostRecipe = recipe;
		ghostInputGroup.inventory.SetStacks(recipe.GetRecipeStacks());
		ghostOutputGroup.inventory.SetStacks(recipe.GetResultStacks());
	}

	private void UpdateGrabUI()
	{
		if (!grabbing) return;
		Vector3 pos = Input.mousePosition;
		pos.z = grabTransform.parent.position.z;
		grabTransform.position = pos;
		grabImg.sprite = sprites.GetItemSprite(grabStack.GetItemType());
		int count = grabStack.GetAmount();
		grabCountText.text = count > 1 ? count.ToString() : string.Empty;
	}

	private void UpdatePreview()
	{
		Item.Type previewType =
			grabbing ? grabStack.GetItemType() : GetInventory(currentContext.context)
			.stacks[currentContext.selected].GetItemType();
		if (previewType == Item.Type.Blank) return;

		if (sprites)
		{
			previewImg.sprite = sprites.GetItemSprite(previewType);
		}
		previewImg.color = previewType == Item.Type.Blank ? Color.clear : Color.white;
		previewName.text = Item.TypeName(previewType);
		previewDesc.text = Item.ItemDescription(previewType);
		previewFlav.text = Item.ItemFlavourText(previewType);
	}

	public void PointerEnter(GameObject slot)
	{
		UpdateSelected(slot);
	}

	public void SlotClickDown(GameObject slot)
	{
		SelectionContext context = FindSlotId(slot);
		Inventory currentInv = GetInventory(context.context);
		if (context.selected < 0 || context.selected >= currentInv.size) return;

		ItemStack inventoryStack = currentInv.stacks[context.selected];
		if (grabStack.GetItemType() == inventoryStack.GetItemType()
			&& !inventoryStack.IsMaxed)
		{
			int leftOver = inventoryStack.AddAmount(grabStack.GetAmount());
			grabStack.SetAmount(leftOver);
		}
		else
		{
			grabStack = currentInv.Replace(grabStack, context.selected);
		}
		grabbing = grabStack.GetItemType() != Item.Type.Blank;
		grabTransform.gameObject.SetActive(grabbing);
		if (context.context == ContextInterface.CraftingInput)
		{
			CheckForMatchInRecipes();
		}
	}

	public void SwitchConstructorRecipeLayout()
	{
		bool goToRecipes = constructorGroup.activeSelf;
		constructorGroup.SetActive(!goToRecipes);
		recipeGroup.SetActive(goToRecipes);
	}

	private void CheckForMatchInRecipes()
	{
		CraftingRecipe? recipe = Crafting.CheckRecipes(craftingInputGroup.inventory.stacks);
		if (recipe != null)
		{
			currentRecipe = (CraftingRecipe)recipe;
			craftingOutputGroup.inventory.SetStacks(currentRecipe.GetResultStacks());
		}
		else
		{
			craftingOutputGroup.inventory.ClearAll();
		}
	}

	public void Collect()
	{
		ItemStack outputStack = craftingOutputGroup.inventory.stacks[0];
		Item.Type outputType = outputStack.GetItemType();
		if (outputType == Item.Type.Blank) return;

		Item.Type grabStackType = grabStack.GetItemType();
		if (grabStackType != outputType && grabStackType != Item.Type.Blank) return;

		int stackLimit = Item.StackLimit(outputType);
		int outputAmount = outputStack.GetAmount();
		int grabStackAmount = grabStack.GetAmount();
		int combinedAmount = outputAmount + grabStackAmount;
		if (grabStackType == Item.Type.Blank || combinedAmount <= stackLimit)
		{
			grabStack.SetItemType(outputType);
			grabStack.AddAmount(outputAmount);
			craftingInputGroup.inventory.RemoveItems(currentRecipe.GetRecipeStacks());
		}
		else if (mainGroup.inventory.CanFit(new ItemStack(outputType, combinedAmount - stackLimit)))
		{
			grabStack.SetItemType(outputType);
			int leftover = grabStack.AddAmount(outputAmount);
			mainGroup.inventory.AddItem(outputType, leftover);
		}
		else
		{
			return;
		}

		grabbing = grabStack.GetItemType() != Item.Type.Blank;
		grabTransform.gameObject.SetActive(grabbing);
		CheckForMatchInRecipes();

		GameEvents.ItemCrafted(outputType, outputAmount);
	}

	private Inventory GetInventory(ContextInterface ci)
	{
		switch (ci)
		{
			case ContextInterface.Inventory: return mainGroup.inventory;
			case ContextInterface.CraftingInput: return craftingInputGroup.inventory;
			case ContextInterface.CraftingOutput: return craftingOutputGroup.inventory;
			case ContextInterface.GhostInput: return ghostInputGroup.inventory;
			case ContextInterface.GhostOutput: return ghostOutputGroup.inventory;
			default: return null;
		}
	}

	private void UpdateSelected(GameObject slot)
	{
		if (grabbing) return;

		SelectionContext context = FindSlotId(slot);
		if (context.selected < 0) return;

		Inventory inv = GetInventory(context.context);
		if (context.selected < 0 || context.selected >= inv.size) return;
		if (inv.stacks[context.selected].GetItemType() == Item.Type.Blank) return;

		currentContext = context;
	}

	private SelectionContext FindSlotId(GameObject slot)
	{
		int i = ContainsSlot(ghostInputGroup, slot);
		if (i >= 0) return new SelectionContext(ContextInterface.GhostInput, i);

		i = ContainsSlot(ghostOutputGroup, slot);
		if (i >= 0) return new SelectionContext(ContextInterface.GhostOutput, i);

		i = ContainsSlot(craftingInputGroup, slot);
		if (i >= 0) return new SelectionContext(ContextInterface.CraftingInput, i);

		i = ContainsSlot(craftingOutputGroup, slot);
		if (i >= 0) return new SelectionContext(ContextInterface.CraftingOutput, i);

		i = ContainsSlot(mainGroup, slot);
		if (i >= 0) return new SelectionContext(ContextInterface.Inventory, i);

		return new SelectionContext(ContextInterface.Inventory, -1);
	}

	private int ContainsSlot(SlotGroup sg, GameObject slot)
	{
		for (int i = 0; i < sg.slots.Count; i++)
		{
			if (sg.slots[i] == slot)
			{
				return i;
			}
		}
		return -1;
	}

	private enum ContextInterface { Inventory, CraftingInput, CraftingOutput, GhostInput, GhostOutput }

	private struct SelectionContext
	{
		public ContextInterface context;
		public int selected;

		public SelectionContext(ContextInterface context, int selected)
		{
			this.context = context;
			this.selected = selected;
		}

		public static bool operator ==(SelectionContext a, SelectionContext b)
			=> a.context == b.context && a.selected == b.selected;

		public static bool operator !=(SelectionContext a, SelectionContext b)
			=> a.context != b.context || a.selected != b.selected;

		public override bool Equals(object obj) => base.Equals(obj);

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString() => $"Slot {selected} of {context}";
	}

	[System.Serializable]
	private class SlotGroup
	{
		public List<GameObject> slots;
		[HideInInspector] public List<Image> slotImages = new List<Image>(50);
		[HideInInspector] public List<Text> slotTexts = new List<Text>(50);
		public Inventory inventory;

		public void FindImagesAndTexts()
		{
			for (int i = 0; i < slots.Count; i++)
			{
				GameObject obj = slots[i];
				slotImages.Add(obj.transform.GetChild(0).GetComponent<Image>());
				slotTexts.Add(obj.transform.GetChild(1).GetComponent<Text>());
			}
		}
	}
}