using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
	[SerializeField] private SlotGroup mainGroup, craftingInputGroup, craftingOutputGroup;

	private SelectionContext currentContext = new SelectionContext(ContextInterface.Inventory, 0);

	private ItemStack grabStack = new ItemStack(Item.Type.Blank, 0);
	private bool grabbing = false;
	[SerializeField] private Transform grabTransform;
	[SerializeField] private Image grabImg;
	[SerializeField] private Text grabCountText;

	[SerializeField] private Camera cam;

	[SerializeField] private Image previewImg;
	[SerializeField] private Text previewName;
	[SerializeField] private Text previewDesc;
	[SerializeField] private Text previewFlav;
	[SerializeField] private ItemSprites sprites;

	[SerializeField] private Button collectButton;
	private CraftingRecipe currentRecipe;

	private void Awake()
	{
		mainGroup.FindImagesAndTexts();
		craftingInputGroup.FindImagesAndTexts();
		craftingOutputGroup.FindImagesAndTexts();
	}

	private void Update()
	{
		UpdateSlots();
		UpdateGrabUI();
		UpdatePreview();
	}

	private void UpdateSlots()
	{
		UpdateSlotGroup(mainGroup);
		UpdateSlotGroup(craftingInputGroup);
		UpdateSlotGroup(craftingOutputGroup);
	}

	private void UpdateSlotGroup(SlotGroup sg)
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
			sg.slotImages[i].color = type == Item.Type.Blank ? Color.clear : Color.white;
			int count = stacks[i].GetAmount();
			sg.slotTexts[i].text = count > 1 ? count.ToString() : string.Empty;
		}
	}

	private void UpdateGrabUI()
	{
		if (!grabbing) return;
		Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
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

	public void PointerClick(GameObject slot)
	{
		SelectionContext context = FindSlotId(slot);
		Inventory currentInv = GetInventory(context.context);
		if (context.selected < 0 || context.selected >= currentInv.size) return;
		grabStack = currentInv.Replace(grabStack, context.selected);
		grabbing = grabStack.GetItemType() != Item.Type.Blank;
		grabTransform.gameObject.SetActive(grabbing);
		if (context.context == ContextInterface.CraftingInput)
		{
			CheckRecipes();
		}
	}

	private void CheckRecipes()
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
		collectButton.interactable = recipe != null;
	}

	public void Collect()
	{
		List<ItemStack> outputStacks = craftingOutputGroup.inventory.stacks;
		mainGroup.inventory.AddItems(outputStacks);
		craftingInputGroup.inventory.RemoveItems(currentRecipe.GetRecipeStacks());
		CheckRecipes();
	}

	private Inventory GetInventory(ContextInterface ci)
	{
		switch (ci)
		{
			case ContextInterface.Inventory: return mainGroup.inventory;
			case ContextInterface.CraftingInput: return craftingInputGroup.inventory;
			case ContextInterface.CraftingOutput: return craftingOutputGroup.inventory;
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
		int i = 0;

		for (i = 0; i < craftingInputGroup.slots.Count; i++)
		{
			if (craftingInputGroup.slots[i] == slot)
			{
				return new SelectionContext(ContextInterface.CraftingInput, i);
			}
		}

		for (i = 0; i < craftingOutputGroup.slots.Count; i++)
		{
			if (craftingOutputGroup.slots[i] == slot)
			{
				return new SelectionContext(ContextInterface.CraftingOutput, i);
			}
		}

		for (i = 0; i < mainGroup.slots.Count; i++)
		{
			if (mainGroup.slots[i] == slot)
			{
				return new SelectionContext(ContextInterface.Inventory, i);
			}
		}
		return new SelectionContext(ContextInterface.Inventory, -1);
	}

	private enum ContextInterface { Inventory, CraftingInput, CraftingOutput }

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

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
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